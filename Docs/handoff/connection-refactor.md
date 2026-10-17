# Handoff: connection-refactor

## Objetivo

`DbSession` abría la conexión **sincrónicamente en su constructor**. Cualquier command que resolviera `IDbSession`
(por `IUnitOfWork` o un repositorio del write side) abría una conexión al construir el handler, aunque fallara antes
de usarla, y el `Open()` bloqueaba un hilo del pool mientras se establecía. Además, `UnitOfWork` usaba
`BeginTransaction`/`Commit`/`Rollback` sincrónicos dentro de un flujo async.

La tarea fue abrir la conexión **de forma lazy y async** (en el primer uso real o al empezar la transacción, nunca en
el constructor) y pasar el `IUnitOfWork` a async, **sin cambiar la semántica de las transacciones** ni los lifetimes
de DI.

## Análisis previo

- **El beneficio es menor de lo que parecía.** La propuesta original decía que la conexión se abría aunque fallara
  la validación de FluentValidation, pero no es así: el endpoint inyecta `ISender` y el handler (con su
  `IUnitOfWork` → `IDbSession`) se resuelve recién en `sender.Send(...)`, después de `ValidateAndThrowExeption`. Las
  queries tampoco usan `IDbSession` (`QueryRepository` crea su propia conexión). La conexión de más solo se abría en
  los commands que fallan en el dominio antes de tocar la base (p.ej. un value object inválido en `LoadPerson` de
  Create o Update).
- **Lo que sí se gana:** el constructor deja de hacer I/O (un `SqlException` ya no salta al resolver el handler desde
  DI) y el `Open()` sincrónico deja de bloquear un hilo.
- **SqlClient 7.1.0 no implementa `BeginTransactionAsync`/`CommitAsync`/`RollbackAsync`/`DisposeAsync`.** Se
  verificó buscando los nombres en `Microsoft.Data.SqlClient.dll` (lib/net8.0): no aparecen. Corren las
  implementaciones base de `DbConnection`/`DbTransaction`, que llaman al método sincrónico y devuelven una task
  completada. El único async de verdad es `OpenAsync`. Igual se pasó el UoW a async: es la API correcta para un repo
  de referencia, y si el provider los implementa se aprovecha sin tocar nada.

## Decisiones tomadas y por qué

1. **`DbSession` crea la conexión en el constructor pero no la abre.** Crear el objeto es barato (no va al servidor),
   así `Connection` sigue siendo no nullable. Se eliminó el try/catch que liberaba la conexión si `Open()` fallaba.
2. **`IDbSession` expone `DbConnection` y `DbTransaction?`** en vez de `IDbConnection`/`IDbTransaction?`, para tener
   las APIs async. `System.Data.Common` es BCL, así que no hay problema de capas (ARCH). Dapper, Dapper.Contrib y
   `CommandDefinition` los aceptan, así que `PersonRepository` y `Repository` no cambiaron.
3. **`IUnitOfWork` async:**
   - `BeginTransactionAsync(CancellationToken)`: abre con `OpenAsync` si la conexión está `Closed` y después
     `BeginTransactionAsync`. El token cancela la apertura y el inicio de la transacción.
   - `CommitAsync()` y `RollbackAsync()` **sin token**: un commit o un rollback no se cancelan a mitad de camino.
     Adentro pasan `CancellationToken.None` explícito, con un `NOTE`.
   - Después del commit/rollback liberan la transacción con `DisposeAsync` (método privado
     `DisposeTransactionAsync`) y la ponen en `null`, igual que antes.
4. **No se agregó `IAsyncDisposable`** a `DbSession` ni a `UnitOfWork`: SqlClient no sobrescribe `DisposeAsync`, así
   que no se gana nada. El `Dispose()` sincrónico de fin de scope queda como estaba.
5. **`CommandHandler.ExecuteInTransaction`** (las dos bases) usa los métodos async. Se mantiene todo lo demás: el
   `ThrowIfCancellationRequested()` antes de empezar, el `BeginTransactionAsync` fuera del `try` (si falla no hay
   nada que deshacer), el rollback en el `catch` y el commit sin cancelación.
6. **Override de `BeginDbTransactionAsync` en `InterceptedDbConnection`.** Sin él, `BeginTransactionAsync` caía en la
   implementación base de `DbConnection`, que llama al `BeginDbTransaction` sincrónico del wrapper, aunque el provider
   lo implemente async.
7. **Fix de `StateChange` en `InterceptedDbConnection`.** El `Dispose` desuscribía `StateChangeHandler`, pero nadie lo
   suscribía nunca: el wrapper nunca disparaba `StateChange`. Se agregó `_connection.StateChange += StateChangeHandler;`
   en el constructor. Hoy nadie usa el evento (Dapper consulta `State`, que ya delegaba bien), pero con la conexión
   lazy es justo lo que uno querría observar.
8. **Las lecturas fuera de transacción dependen del auto-open de Dapper.** `InterceptedDbConnection` hereda de
   `DbConnection` y delega `State`, `Close` y `OpenAsync`, y `TryOpenAsync` de Dapper usa `DbConnection.OpenAsync`.
   `QueryMultipleAsync` con la conexión cerrada la mantiene abierta hasta liberar el `GridReader` (`GetByIdAsync` usa
   `await using`). `InterceptedDbCommand` sobrescribe los `Execute*Async`, así que no hay vuelta a sync.

**Cambio de comportamiento (no de semántica):** antes cada request usaba una sola conexión abierta en el constructor.
Ahora, en Update y Delete:

1. `GetByIdAsync` (fuera de transacción) la abre con Dapper y la cierra (vuelve al pool).
2. `BeginTransactionAsync` la abre de nuevo.
3. Después del commit queda abierta hasta que se libera el scope, y la relectura de Create/Update la usa con
   `Transaction = null`.

Son dos checkouts del pool en vez de uno: despreciable con pooling.

## Qué se hizo

Todo en un solo commit en `feature/mediator` (incluye el fix de `StateChange`).

| Archivo | Cambio |
|---|---|
| `BuildingBlocks.Application.Persistence.Abstractions/IDbSession.cs` | `DbConnection`/`DbTransaction?` y un `<remarks>` que dice que la conexión arranca cerrada |
| `BuildingBlocks.Application.Persistence.Abstractions/IUnitOfWork.cs` | `BeginTransactionAsync(CancellationToken)`, `CommitAsync()`, `RollbackAsync()` |
| `BuildingBlocks.Infra.Persistence/DbSession.cs` | Crea la conexión sin abrirla, sin try/catch, con un `NOTE` |
| `BuildingBlocks.Infra.Persistence/UnitOfWork.cs` | `OpenAsync` si está cerrada, métodos async y `DisposeTransactionAsync` privado |
| `BuildingBlocks.Application/Cqrs/CommandHandler.cs` | Los dos `ExecuteInTransaction` con `await` de los métodos async y el `<remarks>` actualizado |
| `Core/My.Data.InterceptableDbConnection/InterceptedDbConnection.cs` | Override de `BeginDbTransactionAsync` y suscripción a `StateChange` en el constructor |

Sin cambios: `PersonRepository`, `Repository`, `AddPersistence` (mismos lifetimes: `IDbSession` e `IUnitOfWork`
scoped, repositorios transient).

## Verificación hecha

- `dotnet build Records.sln -p:CI=true --no-incremental`: **0 warnings, 0 errores**.
- No queda ninguna llamada a `Open()`, `BeginTransaction()`, `Commit()` ni `Rollback()` sincrónicos. El único
  `Open()` es el override del wrapper, que `DbConnection` exige.
- API levantada **sin SQL Server**:
  - `/openapi/v1.json` → 200 (el cableado de DI está bien).
  - `DELETE /persons/{id}` → 500 por la base inaccesible. El stack trace muestra que la conexión la abre Dapper con
    `InterceptedDbConnection.OpenAsync` dentro de `PersonRepository.GetByIdAsync`, y no el constructor de
    `DbSession`.
- **Nada se probó contra una base real**: ni Create/Update/Delete/GetById, ni el commit, ni el rollback.

## Pendiente

- **Probar contra SQL Server** (base creada con `RecordsPersons_Source.sql`).
- **Handoff `person-repository` desactualizado** en una justificación: dice que `Connection` es no nullable porque
  "`DbSession` siempre la asigna en el ctor (si `Open()` falla, tira)". Sigue siendo no nullable (se crea en el
  constructor), pero ya no se abre ahí. El gotcha de no anidar `ExecuteInTransaction` sigue vigente.
- **Handoff `cancelationtoken-solo-hasta-commit`:** su pendiente "`BeginTransaction`/`Commit`/`Rollback` async" quedó
  resuelto con esta tarea.
- **Fila en la tabla de handoffs del `CLAUDE.md`** para este handoff. No se agregó.
- **`IUnitOfWork` tiene una region `Private methods`** con métodos públicos, y la skill `team-standards` dice que las
  interfaces no llevan regions. Ya estaba así, no se tocó.
- **Fuera de alcance, sin cambios:** si `RollbackAsync` tira en el `catch`, tapa la excepción original (ya pasaba
  antes; cambiarlo cambiaría la semántica).

## Próximos pasos

1. Configurar la connection string en `appsettings.Development.json` contra una base creada con
   `RecordsPersons_Source.sql` y probar:
   - Create (con address y assets), GetById, Update (agregando un asset) y Delete.
   - Un rollback forzado (p.ej. renombrar temporalmente una columna de `Addresses` y hacer un POST): no tiene que
     quedar la fila en `Persons`.
2. Con un `StateChange` suscripto (o un breakpoint en `StateChangeHandler`), confirmar la secuencia esperada en un
   Update: abrir/cerrar en `GetByIdAsync`, abrir en `BeginTransactionAsync` y cerrar al liberar el scope.
3. Actualizar `person-repository` y `cancelationtoken-solo-hasta-commit` con `/handoff` cuando se retome esa área.

## Gotchas

- **SqlClient 7.1 no tiene transacciones async reales.** Los `await` de `BeginTransactionAsync`/`CommitAsync`/
  `RollbackAsync`/`DisposeAsync` corren sincrónicos por debajo. El criterio "no hay llamadas sincrónicas" se cumple
  en el código, no en lo que pasa por debajo. Para verificarlo en otra versión: buscar `CommitAsync` dentro del
  `Microsoft.Data.SqlClient.dll` (`grep -c -a`). Cargarlo por reflection desde Windows PowerShell 5.1 no funciona
  (es .NET Framework y la dll es net8.0).
- **Un wrapper de `DbConnection` tiene que sobrescribir las variantes async** (`OpenAsync`,
  `BeginDbTransactionAsync`): si no, la implementación base llama a la sincrónica sin avisar.
- **Las escrituras tienen que ir siempre dentro de `ExecuteInTransaction`.** Ahí la conexión ya está abierta. Fuera
  de una transacción dependerían del auto-open de Dapper.Contrib, que no se verificó para sus métodos async.
- **`launchSettings.json` de la API tiene un comentario** que el `dotnet run` no puede parsear ("'/' is an invalid
  start of a property name"): el perfil no se aplica. Para levantarla desde la shell se usó
  `dotnet run --no-build --project Src/Apps/Records.Persons.Api.V1 --urls http://localhost:5199`.
- **Finales de línea:** los archivos tocados conservaron su EOL y BOM previos en la working copy (algunos LF, otros
  CRLF; el índice guarda LF). Verificar con `git ls-files --eol <archivos>`.
