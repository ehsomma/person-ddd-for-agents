# Handoff: person-repository

## Objetivo

Implementar la persistencia del agregado `Person` (persona + `Address` + `PersonalAssets`) con Dapper sobre
SQL Server (`RecordsPersons_Source.sql`), cableada de punta a punta desde la API (endpoint → mediator →
`CreatePersonCommandHandler` → `IUnitOfWork` → `PersonRepository`), con transacción y rollback explícitos.

## Qué se hizo

Todo commiteado y pusheado en `feature/mediator` (`43769c3` … `da9390f`).

| Archivo / área | Cambio | Por qué |
|---|---|---|
| `IDbSession` / `DbSession` / `UnitOfWork` (BuildingBlocks.Infra.Persistence*) | `Connection` pasó a no nullable (`IDbConnection`); se quitaron los `?.` | `DbSession` siempre la asigna en el ctor (si `Open()` falla, tira). Era la causa del warning CS8604 en el repo |
| `UnitOfWork.Dispose()` | `_session.Transaction = null` después del dispose | Un repo usado después del commit/rollback recibía una transacción ya liberada |
| `CommandHandler.cs` (BuildingBlocks.Application/Cqrs, las 2 bases) | Nuevo `protected async Task ExecuteInTransaction(Func<Task> operation)`: begin → operación → commit; `catch` → `Rollback()` + `throw` | `Rollback()` existía pero **nadie lo llamaba** (nunca, desde `beffbff`). No quedaban datos a medias solo porque el dispose de la conexión descartaba la transacción |
| `CreatePersonCommandHandler` | `await ExecuteInTransaction(() => _personRepository.InsertAsync(person));` | Los domain events se publican después, solo si hubo commit |
| `Records.Persons.Api.V1` (`.csproj` + `Program.cs`) | Referencia a `Records.Persons.Infra.Persistence.Sql`; `AddPersistence(typeof(PersonRepository).Assembly)`; `services.AddMappers(...)` | `IPersonRepository` no estaba registrado → la API **no arrancaba** en Development (`ValidateOnBuild`) |
| `BuildingBlocks.Infra.Mappings.DependencyInjection` (nuevo, del usuario) | Solo `AddMappers` (scan de clases `*Mapper`, `StringComparison.Ordinal`) | Se eliminó `AddMapster` y los paquetes de Mapster |
| `Records.Persons.Infra.Persistence.Sql/PersonAggregate/` (del usuario, revisado) | `Models/` (DataModels con atributos de Dapper.Contrib), `Mappers/PersonMapper.cs`, `Repository/PersonRepository.cs` | Enfoque DataModel + mapper del repo anterior del usuario |
| `PersonMapper.FromDomainToDataModel` | Reescrito **a mano** (antes Mapster + `PersonMapperConfig`, borrado) | El mapeo no es 1:1 (VOs→primitivos, `Gender`→`Name`, `LatLng`→`Lat`/`Lng`, `PersonId` en los hijos); con Mapster el `PersonId` de los assets no salía (`FIX2 not working`) y el de la address dependía de una config dudosa → riesgo de `Guid.Empty` y violación de FK. A mano: chequeo en compilación |
| `PersonMapper.FromDataModelToDomain` | `LatLng` solo si `Lat` **o** `Lng` tienen valor (`||`) | `LatLng.Build(null, null)` tira. Con `||` una fila con una sola coordenada (dato corrupto) tira en vez de descartarse en silencio |
| `City`, `State`, `CountryName` (dominio, cambio del usuario) | Pasaron a `StringValueObjectNullable` | `StringValueObject.Build(null)` tira y esas columnas son `NULL` en la tabla |
| `RecordsPersons_Source.sql` | `CK_Addresses_LatLng`: `Lat` y `Lng` ambos NULL o ambos con valor | Misma regla que el dominio, también en la base |
| `PersonRepository.InsertAsync` | Dapper.Contrib; personal assets con `foreach` + `await` **secuencial** | Venía con `Task.WhenAll` sobre la misma conexión: `SqlConnection` no soporta comandos concurrentes (MARS no lo resuelve) |
| `PersonRepository.GetByIdAsync` | `QueryMultipleAsync` con 3 SELECT (columnas explícitas) en un round trip → `FromDataModelToDomain`; `null` si no existe | `PersonService` convierte el `null` en `DomainErrors.Person.NotFound` |
| `PersonRepository.UpdateAsync` | Contrib `UpdateAsync` de persona y address + `InsertAsync` de los assets con `Id == 0` | El dominio no actualiza ni borra assets (TODO en `Person.Update`), solo agrega con `AddPersonalAsset`. Decisión aceptada por el usuario |
| `PersonRepository.DeleteAsync` | Un batch: `DELETE` de `PersonalAssets`, `Addresses` y `Persons` por `PersonId`/`Id` | Hijos primero: las FK no tienen `ON DELETE CASCADE`. Borrado físico (no hay columna de estado) |
| `.editorconfig` | IDE0300/0301/0303/0304/0305 en `silent` (IDE0306 ya estaba) | El usuario no usa collection expressions; la categoría Style está forzada a `warning` |
| `.claude/skills/team-standards/SKILL.md` | Nueva regla "Nombres cuando conviven la misma entidad de distintas capas" | Tipos con using aliases de capa (`DomainModel`, `DataModel`, `Dto`) terminados en `// Using aliases.`; variable del dominio sin prefijo (`person`), la otra con prefijo (`dataPerson`, `personDto`); excepción: mappers (`domainPerson`/`dataPerson`). Resuelve CA1725 |

## Verificación hecha

- `dotnet build Records.sln --no-incremental`: **0 warnings, 0 errores**.
- La API arranca en Development sin errores de DI; `/openapi/v1.json` → 200.
- POST `/persons/` sin base: recorre endpoint → mediator → handler → UoW y falla al abrir la conexión
  (esperado, no hay SQL Server).
- **Nada del repositorio se probó contra una base real** (no existe todavía).

## Pendiente

- Probar contra la base creada con `RecordsPersons_Source.sql` (incluye el CHECK nuevo).
- No hay endpoints/handlers de lectura, update ni delete: `GetByIdAsync`/`UpdateAsync`/`DeleteAsync` solo
  se usan desde `PersonService.GetByIdAsync` (sin endpoint todavía).
- `UpdateAsync` no controla el `bool` de Contrib (fila inexistente → no avisa). El usuario lo dejó así.
- `CreatePersonCommandHandler`: `_settings` sin uso, mantenido a propósito como ejemplo (comentado).

## Próximos pasos

1. Crear la base y probar el ciclo completo: POST con address (con y sin `City`/`LatLng`) y 2 assets →
   verificar filas y `PersonId` en `Addresses`/`PersonalAssets`.
2. Probar el rollback: renombrar temporalmente una columna de `Addresses`, hacer el POST y verificar que
   no quedó la fila en `Persons`.
3. Agregar los commands/queries + endpoints de get by id, update y delete, y probar `GetByIdAsync`,
   `UpdateAsync` (agregando un asset) y `DeleteAsync`.

## Gotchas

- **Dapper no aplica implicit operators**: en un objeto anónimo `new { fullName = person.FullName }` el
  tipo es `FullName` y Dapper tira `NotSupportedException`. Usar `.Value` (hoy se evita con el mapper).
- **No anidar `ExecuteInTransaction`** (segundo `BeginTransaction` sobre la misma `SqlConnection` →
  `InvalidOperationException`) ni llamarlo dos veces esperando atomicidad: son dos transacciones. Para
  varias operaciones, pasar una lambda `async` con todas adentro.
- **Nunca `Task.WhenAll` sobre `_dbSession.Connection`**: mismo motivo que en `Mediator.Publish`.
- **Dapper.Contrib con IDENTITY**: `[Key]` (no `[ExplicitKey]`) en `Address`/`PersonalAsset.Id`; en
  `Person.Id` (Guid) va `[ExplicitKey]`. Contrib `UpdateAsync` reescribe todas las columnas (incluido
  `CreatedOnUtc`, con el valor cargado).
- **`StringValueObject` vs `StringValueObjectNullable`**: el primero tira con `null`; para columnas NULL
  usar el segundo.
- **`Gender` se guarda por `Name`** (`varchar(10)`: "Male"/"Female"/"Other") y se lee con `Gender.FromName`.
- **ValidateOnBuild en Development** detecta registros faltantes al arrancar: levantar la API es la forma
  rápida de validar el cableado de DI sin base.
- El skill `git-commit` exige mostrar el mensaje y esperar OK antes de cada commit, y **no** agregar
  trailers `Co-Authored-By`.
