# Handoff: cancelationtoken-solo-hasta-commit

## Objetivo

El read side propagaba el `CancellationToken` hasta Dapper con `CommandDefinition`, pero el write side no:
no lo recibían `CommandHandler.ExecuteInTransaction`, `IPersonRepository`, `IPersonService` ni sus
implementaciones. Como este repo es la referencia que copian los agentes, la inconsistencia se iba a replicar
en cada módulo nuevo.

La tarea fue llevar el token hasta el repositorio en el write side, y definir **hasta dónde** se respeta la
cancelación: hasta el commit, y no después.

## Análisis previo: cómo se comportaba el token

Antes del cambio, el token del request (`HttpContext.RequestAborted`, enlazado por Minimal APIs en el
endpoint) llegaba al handler, pero solo se reenviaba a `PublishDomainEvents`. De ahí pasaba por
`DomainEventPublisher` → `Mediator.Publish` → `NotificationHandler.Handle`, y nadie lo consultaba. Los cuatro
event handlers lo ignoran (solo loguean). Resultado: cancelar no tenía ningún efecto, y el handler siempre
corría completo.

El riesgo aparecía a futuro: si un event handler empezaba a usar el token (un `HttpClient`, un Dapper con
`CommandDefinition`), una cancelación después del commit cortaba el `foreach` secuencial de
`DomainEventPublisher` a la mitad. La persona quedaba commiteada, una parte de los eventos se perdía y no había
forma de reintentarlos.

## Decisiones tomadas y por qué

1. **Todo método async del write side recibe `CancellationToken cancellationToken = default` como último
   parámetro.** Aplica a las interfaces del dominio, sus implementaciones y `CommandHandler`. Así queda igual
   que el read side.
2. **`ExecuteInTransaction(Func<Task> operation, CancellationToken cancellationToken = default)`:**
   - Llama a `ThrowIfCancellationRequested()` **antes** de `BeginTransaction`: si ya está cancelado, no hay
     nada que deshacer. **Actualizado por `connection-refactor`:** ahora es `BeginTransactionAsync`, que además
     recibe el token, así que una cancelación también corta la apertura de la conexión y el inicio de la
     transacción (fuera del `try`, sin nada que deshacer). `CommitAsync`/`RollbackAsync` no reciben token.
   - El token se pasa también dentro del lambda:
     `ExecuteInTransaction(() => _personRepository.DeleteAsync(person, cancellationToken), cancellationToken)`.
   - Una cancelación durante la operación hace rollback por el `catch` que ya existía.
   - **El commit no se cancela:** si la operación terminó bien, se commitea siempre.
3. **Dapper.Contrib (`InsertAsync`/`UpdateAsync`) no acepta `CancellationToken`.**
   - No se migró a Dapper puro.
   - Antes de cada llamada a Contrib hay un `cancellationToken.ThrowIfCancellationRequested()`. El `NOTE` que
     lo explica está en `InsertAsync`, y `UpdateAsync` remite a él.
   - Donde ya se usaba Dapper puro (`DeleteAsync` con `ExecuteAsync`, `GetByIdAsync` con
     `QueryMultipleAsync`), se pasó a `CommandDefinition` con el token, como en el read side.
4. **`PublishDomainEvents` dejó de recibir el token** y adentro publica con `CancellationToken.None`.
   - El plan original decía "no cambies `PublishDomainEvents`" y la primera versión pasaba
     `CancellationToken.None` desde cada handler. Después, con el OK del usuario, se sacó el parámetro.
   - **Por qué se sacó:** con el parámetro, si alguien omitía el argumento, CA2016 daba warning (`Handle` tiene
     un token sin reenviar). El code fix, o un agente que busca 0 warnings, pasaba el token del request, que es
     justo lo que no se quiere. Sin el parámetro la decisión queda en un solo lugar y nadie puede equivocarse.
   - `IDomainEventPublisher.Publish` **conserva** su token: es una abstracción genérica, y el que decide no
     cancelar es el flujo del command.
5. **La relectura después del commit sí recibe el token del request.** Es el `GetByIdAsync` de `CreatePerson`
   y `UpdatePerson`: si el cliente ya se fue, cortar ahí no tiene costo, porque el dato ya está guardado.
6. **No se agregó una regla a la skill `team-standards`.** Lo de los eventos ya no se puede violar (el método no
   tiene parámetro) y es una decisión de diseño, no de estilo, así que quedó en "Decisiones de diseño" del
   CLAUDE.md.
   - Se evaluó una regla general ("todo async del write side recibe el token como último parámetro"), porque
     CA2016 verifica el reenvío pero no que el parámetro exista.
   - Quedó para agregarla solo si en la práctica los agentes se olvidan.

## Qué se hizo

Commit `24f5adf` (`feat: propaga el CancellationToken hasta los repositorios del write side`) en
`feature/mediator`. **No está pusheado** (sigue sin pushear al cerrar `connection-refactor`).

| Archivo | Cambio |
|---|---|
| `Src/BuildingBlocks/BuildingBlocks.Application/Cqrs/CommandHandler.cs` | En las dos clases: `ExecuteInTransaction` recibe el token y lo chequea antes de `BeginTransaction` (con docs en `<remarks>` y `<exception>`). `PublishDomainEvents` pierde el parámetro y publica con `CancellationToken.None`, con un `NOTE` |
| `Records.Persons.Domain/PersonAggregate/Repositories/IPersonRepository.cs` | Los cuatro métodos reciben el token |
| `Records.Persons.Domain/PersonAggregate/Services/IPersonService.cs` y `PersonService.cs` | `GetByIdAsync` recibe el token y lo reenvía al repositorio |
| `Records.Persons.Infra.Persistence.Sql/PersonAggregate/Repository/PersonRepository.cs` | `ThrowIfCancellationRequested()` antes de cada llamada a Contrib. `CommandDefinition` con el token en `DeleteAsync` y `GetByIdAsync` |
| `Records.Persons.Application/Person/Commands/{Create,Update,Delete}Person/*CommandHandler.cs` | Pasan el token a `GetByIdAsync`, al repositorio dentro del lambda y a `ExecuteInTransaction`. `PublishDomainEvents` se llama sin token |
| `CLAUDE.md` | En "Decisiones de diseño": los eventos se publican sin token, y el token llega hasta el repositorio en los dos lados, con el chequeo previo para Contrib |

Sin cambios:
- **`BuildingBlocks.Infra.Persistence/Repository.cs`:** no tiene métodos async.
- **Event handlers:** solo loguean y no llaman a nada async.

## Verificación hecha

- `dotnet build Records.sln -p:CI=true`: **0 warnings, 0 errores**. Eso incluye CA2016.
- **No se probó en ejecución** ningún escenario de cancelación.

## Pendiente

- **Probar la cancelación en ejecución.** Ver los próximos pasos.
- **Outbox.** Sacar el token de los eventos evita que una desconexión del cliente los deje a medias, pero no
  cubre que un event handler tire una excepción después del commit (hoy el cliente recibe un 500 con el dato
  guardado), ni un crash del proceso. La garantía real es guardar los eventos en la misma transacción y
  despacharlos aparte con reintentos. El propio `PersonCreatedEventHandler` ya lo menciona en un comentario.
- **Migrar Dapper.Contrib a Dapper puro**, para que el token interrumpa también un comando en curso y no solo
  el siguiente. Se decidió no hacerlo en esta tarea.
- ~~**`BeginTransaction`/`Commit`/`Rollback` async** (DbSession/UoW).~~ **Resuelto** en `connection-refactor`.
- ~~**Fila en la tabla de handoffs del `CLAUDE.md`** para este handoff.~~ **Resuelto:** ya está en la tabla.

## Próximos pasos

1. Levantar la API y forzar una cancelación con un delay de prueba en el repositorio y un cliente que corte la
   conexión. Verificar tres casos:
   - **Cancelación antes de `ExecuteInTransaction`:** no se abre la transacción y no hay cambios.
   - **Cancelación entre dos llamadas a Contrib en `InsertAsync`:** hay rollback y no queda ninguna fila parcial
     en `Persons`, `Addresses` ni `PersonalAssets`.
   - **Cancelación después del commit:** los eventos se publican igual. Solo puede fallar la relectura, y el dato
     queda guardado.
2. Con un `OperationCanceledException` de un request abortado, revisar qué responde y qué loguea. Ver el gotcha
   sobre el `ExceptionHandlerMiddleware`.
3. Si se aborda el Outbox, hacerlo en `BuildingBlocks` (en `ExecuteInTransaction` o el UoW), no en cada módulo.

## Gotchas

- **La herramienta de edición dejó los `.cs` en LF en la working copy.** El índice guarda LF, así que el commit
  no cambiaba, pero la convención local es CRLF. Se reconvirtieron con `sed -i 's/$/\r/'` sobre los archivos con
  `w/lf`.
  - Para verificarlo, usá `git ls-files --eol <archivos>`.
  - No uses `grep -c $'\r$'` en Git Bash: dio que todos tenían CRLF cuando no era así.
- **CA2016 empuja en la dirección equivocada** si un método post-commit acepta un token opcional: sugiere
  reenviar el del request. Esa fue la razón para sacar el parámetro de `PublishDomainEvents` en vez de documentar
  "pasá `CancellationToken.None`".
- **`SqlMapper.GridReader` no recibe token** en `ReadSingleOrDefaultAsync`/`ReadAsync`: usa el que se pasó en el
  `CommandDefinition` del `QueryMultipleAsync`.
- **Los `OperationCanceledException` de un request abortado** probablemente no lleguen a `GlobalExceptionHandler`.
  Según el comportamiento de .NET 8+, el `ExceptionHandlerMiddleware` los corta antes, los loguea en debug y
  responde 499. **No está verificado en este repo.**
- **El diagnóstico de VS "Null check can be simplified"** en `PersonService.GetByIdAsync` ya estaba antes. No es
  de esta tarea y el build no lo marca.
