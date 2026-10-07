# Handoff: usecase-delete-person

## Objetivo

Implementar el caso de uso `DeletePerson` de punta a punta (Api → Command/Application → Domain → Repository),
tomando como modelo lo que se hizo con `UpdatePerson`. Antes de implementarlo había que decidir cómo verificar
que la persona existe sin depender de un error de la base de datos.

## Decisión de diseño: cargar el agregado completo (no `CheckExists()`)

El handler obtiene el agregado entero con `PersonService.GetByIdAsync`. Si no existe, el servicio lanza
`DomainErrors.Person.NotFound` y se devuelve un 404. Se descartó un `Exists()` que devuelva true/false por
estas razones:

1. **Las reglas de negocio del borrado van en el agregado.** Si mañana existe una regla como "no se puede
   eliminar si ...", va dentro de `Person.Delete()`, y para eso hace falta la entidad cargada. Con `Exists()`
   la regla terminaría en el handler.
2. **Los eventos salen del agregado:** `PersonDeletedEvent` recibe el `Person`.
3. **Es el mismo flujo que Create y Update:** cargar → comportamiento → persistir → publicar eventos. El 404
   sale del mismo lugar.
4. **El repositorio queda como una colección de agregados** (`GetById`/`Insert`/`Update`/`Delete`), sin
   consultas sueltas.

El costo es una lectura de más (persona, address y assets en un solo round trip con `QueryMultiple`), y en
un delete por ID es despreciable. `Exists()` o un `DELETE` con filas afectadas solo se justificaría en
borrados masivos o en hot paths, y únicamente si no hay reglas de negocio ni eventos con estado.

## Qué se hizo

Commit `6cbb3f3` (`feat(persons): agrega el caso de uso de delete de persona`), pusheado en
`feature/mediator`.

`PersonRepository.DeleteAsync` (borrado físico: PersonalAssets → Addresses → Persons, porque las FKs no
tienen cascade) y `Person.Delete()` (registra `PersonDeletedEvent`) **ya existían** desde `da9390f`.

| Archivo | Cambio |
|---|---|
| `Application/Person/Commands/DeletePerson/DeletePersonCommand.cs` (nuevo) | `ICommand` sin respuesta, con `AppKey` e `Id` |
| `Application/Person/Commands/DeletePerson/DeletePersonCommandHandler.cs` (nuevo) | Usa `CommandHandler<DeletePersonCommand>` (la variante sin `TResponse`). Hace `GetByIdAsync` → `person.Delete()` → `ExecuteInTransaction(DeleteAsync)` → `PublishDomainEvents`. No usa mapper porque no devuelve nada |
| `Api.V1/Person/DeletePerson/DeletePersonEndpoint.cs` (nuevo) | `DELETE /persons/{id:guid}` (name `DeleteDeletePerson`, por la convención `<Verbo><CasoDeUso>`), devuelve `TypedResults.NoContent()` (204). El 404 lo resuelve `GlobalExceptionHandler` según `ErrorType.NotFound` |

A diferencia del Update, el endpoint no devuelve la entidad (ya no existe) y no hay chequeo de ID entre ruta
y body porque no hay body.

## Verificación hecha

- `dotnet build` de la solución: **0 warnings, 0 errores**.
- **No se probó el endpoint en ejecución.**

## Pendiente

- Probar en ejecución: un DELETE de una persona existente (204, y que se borren sus filas en `Persons`,
  `Addresses` y `PersonalAssets`) y un DELETE de un ID inexistente (404 con `ERR.PERSON.NOTFOUND`).
- `Person.Delete()` todavía tiene el placeholder `// Do some validation/enseres.`: ahí van las reglas de
  negocio del borrado cuando se definan (o el cambio de estado si se pasa a borrado lógico).
- Concurrencia: entre el `GetById` y el `DELETE`, otro request podría borrar la misma persona. Hoy el
  `DELETE` afecta 0 filas sin error y el resultado final es el mismo. Si importa detectarlo, la solución es
  concurrencia optimista (rowversion) en todos los comandos, no `Exists()`.

## Próximos pasos

1. Levantar la API y probar los dos casos de arriba desde Scalar (`/scalar/v1`).
2. Si se definen reglas de negocio para el borrado, implementarlas en `Person.Delete()` con su
   `DomainErrors.Person.*` (con el `ErrorType` correspondiente, p.ej. `Conflict` → 409).

## Gotchas

- Quedó **sin commitear, a propósito**, un cambio en `Records.Persons.Application.csproj`
  (`<Folder Include="Person\Events\PersonCreated\" />`). Lo agregó el IDE al crear una carpeta vacía y no es
  parte de esta tarea: el usuario lo commitea después.
- Los archivos nuevos se crearon con LF. Git avisa que los va a convertir a CRLF, pero es inofensivo.
