# Handoff: queries

## Objetivo

Implementar el lado de lectura (queries) de `Person`, tomando como base el repo de referencia
([`ehsomma/ddd-cqrs-microservices`](https://github.com/ehsomma/ddd-cqrs-microservices), proyecto
`Records.Persons.Infra.Reads.sql`), pero más simple:

- Una sola base de datos, sin Projection.
- Las queries devuelven **DTOs** (`Records.Persons.Dtos`), no ReadModels.
- Flujo: el API arma la query → el handler de Application la manda al query repository → el repo usa
  Dapper y mapea **directo a los DTOs** (sin data model ni mappers intermedios).
- Todos los query repositories heredan de una base común que setea la conexión y obliga a implementar
  `GetAsync`.

Se implementaron 4 casos de uso, uno por cada escenario de firma de endpoint: `GetPersonById` (ruta),
`GetPersons` (sin parámetros), `GetPersonsByGenderAndBirthdate` (query string) y
`GetPersonsWithSpecificProfile` (POST con la query en el body).

## Qué se hizo

Commits en `feature/mediator` (sin push):

| Commit | Contenido |
|---|---|
| `3e10586` | `feat(persons)`: base de query repositories + `GetPersonById`. |
| `6b5f9ca` | `style`: `new` con tipo explícito + estándares de `new` y `NOLOCK` en `team-standards`. |
| `c61de41` | `feat(persons)`: `GetPersons`, `GetPersonsByGenderAndBirthdate`, `GetPersonsWithSpecificProfile` + `QueryPersonsAsync`. |
| `9272d4f` | `fix(http)`: `BadHttpRequestException` → su status code (400) en vez de 500. |

### Archivos

| Archivo | Cambio | Por qué |
|---|---|---|
| `BuildingBlocks.Infra.Persistence.Abstractions/IQueryRepository.cs` | `IQueryRepository<in TQuery, TResponse>` con `GetAsync(TQuery, CancellationToken)`. | Puerto que inyectan los handlers. Solo existe la variante **con** query (ver decisiones). |
| `BuildingBlocks.Infra.Persistence/QueryRepository.cs` | Base abstracta: lee `SqlServerSettings.SourceConnectionString` de `IConfiguration`, expone `protected DbConnection CreateConnection()` (devuelve un `InterceptedDbConnection` cerrado) y `public abstract GetAsync`. | La conexión se setea en un solo lugar y cada repo está obligado a implementar `GetAsync`. |
| `Records.Persons.Infra.Queries.Sql/` (proyecto nuevo) | Referencia a Application, Dtos, Dapper y BuildingBlocks.Infra.Persistence. Agregado al `.sln` a mano, en la carpeta `Persons/Infra`. | El lado de lectura necesita conocer Application (query + puerto). Así `Persistence.Sql` (escritura) sigue dependiendo solo del dominio. |
| `.../Person/PersonMultiMappingExtensions.cs` | `connection.QueryPersonsAsync(command)`: multi-mapping de Person, Address, LatLng y PersonalAsset (`splitOn: "Id,Lat,Id"`) y agrupación por persona. | Las 4 queries leen lo mismo; cada repo mantiene su SQL completo y solo comparte el mapeo. El `<remarks>` documenta el orden de columnas obligatorio. |
| `.../Person/GetPersonById/GetPersonByIdRepository.cs` | SQL con JOINs + `QueryPersonsAsync` + `SingleOrDefault()`. Devuelve `null` si no existe. | |
| `.../Person/GetPersons/`, `GetPersonsByGenderAndBirthdate/`, `GetPersonsWithSpecificProfile/` | Un repo por query, cada uno con su SQL. Devuelven `IList<Dto.Person>` (vacía si no hay resultados). | |
| `Records.Persons.Application/Person/Queries/*/` | Query + handler por caso de uso. Los handlers inyectan `IQueryRepository<TQuery, TResponse>` directo. `GetPersonByIdQueryHandler` lanza `DomainException(DomainErrors.Person.NotFound)` si el repo devuelve `null`. | Mismo 404 y mismo código (`ERR.PERSON.NOTFOUND`) que los commands. |
| `Records.Persons.Api.V1/Person/Get*/` | 4 endpoints: `GET /persons/{id}`, `GET /persons`, `GET /persons/by-gender-and-birthdate`, `POST /persons/with-specific-profile`. | |
| `Records.Persons.Api.V1/Program.cs` / `.csproj` | Referencia al proyecto nuevo; `AddPersistence(typeof(PersonRepository).Assembly, typeof(GetPersonByIdRepository).Assembly)`. | Scrutor registra las clases `*Repository` con `AsImplementedInterfaces`. |
| `Records.Persons.Dtos/Person/Person.cs`, `Address.cs` | `Person.Address`, `Person.PersonalAssets` y `Address.LatLng` pasan de `init` a `set`. `PersonalAssets` con `SuppressMessage` de CA2227. | Ver decisiones. |
| `BuildingBlocks.Infra.Http/GlobalExceptionHandler.cs` | Si la excepción es `BadHttpRequestException`, usa su `StatusCode` y el código `ERR.VALIDATION`. | Faltaba un parámetro o el JSON era inválido → 500. Afectaba también a `CreatePerson`. |
| `.editorconfig` | `csharp_style_implicit_object_creation_when_type_is_apparent = false`, `IDE0090 = none`. | Preferencia del usuario: tipo explícito en el `new`, mismo criterio que con `var`. |
| `.claude/skills/team-standards/SKILL.md` | 2 estándares nuevos: tipo explícito en el `new`, y `WITH (NOLOCK)` en todos los `SELECT` (con la excepción de la carga de aggregates para commands). | |
| 15 archivos varios | `new(...)` → `new Tipo(...)`. | Nuevo estándar. |

### Decisiones (discutidas con el usuario)

- **DTOs como clases, no records.** Al principio se usaron records, para componer con `with` sin tocar setters.
  El usuario prefiere clases. Dapper asigna las propiedades `init` por reflection sin problema; lo que no
  compila es la lambda del multi-mapping (`address.LatLng = latLng` → CS8852). Por eso las tres propiedades
  de navegación son `set`, igual que en el repo de referencia. Con `AnalysisMode=All`, `PersonalAssets { set; }`
  dispara CA2227 → `SuppressMessage` con justificación. No se inicializó la lista con `= new List<>()`,
  porque cambiaría los commands (`[]` en vez de `null`).
- **Sin interfaz específica por repo** (`IGetPersonByIdRepository` del repo de referencia): el handler inyecta
  `IQueryRepository<GetPersonByIdQuery, Dto.Person?>` directo.
- **Se eliminó la variante sin query (`IQueryRepository<TResponse>`):** dos repos que devuelven el mismo tipo
  (p.ej. `GetPersons` y `GetActivePersons`, ambos `IList<Dto.Person>`) serían ambiguos en DI. La query
  siempre es obligatoria, aunque esté vacía (`GetPersonsQuery`). Igual tiene que existir: el mediator
  resuelve el handler por el tipo de la query.
- **Conexión propia en vez de `IDbSession`:** al principio se reusaba el `IDbSession` scoped (la conexión de
  los commands). El usuario prefirió la alternativa: cada `GetAsync` hace
  `await using DbConnection connection = CreateConnection();`. Así las queries no dependen de la unidad de
  trabajo, y con `NOLOCK` no se bloquean contra transacciones abiertas.
- **`WITH (NOLOCK)` en todos los SELECT**, salvo los que cargan un aggregate para un command
  (`PersonRepository.GetByIdAsync`): ahí una dirty read de un cambio que después hace rollback quedaría
  persistida.
- **Una sola consulta con JOINs + agrupación**, como en el repo de referencia. Dapper no tiene multi-mapping
  async sobre `QueryMultiple`.
- **Gender como `DbString { IsAnsi = true, Length = 10 }`:** la columna es `varchar`, y un parámetro
  `nvarchar` (el default de Dapper para `string`) convierte la columna e impide usar un índice.
- **`GetPersonsWithSpecificProfile`:** la query llega directo como `[FromBody]`. `System.Text.Json` usa el
  constructor, así las propiedades quedan `{ get; }`. Tiene `<example>` para OpenAPI. Se corrigió un bug del
  repo de referencia: `DATEADD(year, @ageOlderThan, ...)` sumaba años; ahora es
  `Birthdate <= DATEADD(YEAR, -@ageOlderThan, hoy)` (edad mínima inclusive). Usa `INNER JOIN` con
  Addresses, porque filtra por ciudad.
- **Nombre `GetPersonsByGenderAndBirthdate`** (no `...Birthday` como pidió el usuario para el endpoint), para
  que coincida con la query: la convención es carpeta = caso de uso = query. Se le avisó al usuario; si
  prefiere `Birthday`, hay que renombrar.

## Verificación hecha

- Build de la solución: 0 errores, 0 warnings.
- Con la API levantada (**sin SQL Server**): los 4 endpoints con datos válidos llegan hasta el `GetAsync` de
  su repo y fallan recién al conectar. Ruteo, binding y DI funcionan.
- OpenAPI: aparecen los 4 paths; el schema del body de SpecificProfile tiene descripciones, ejemplos y
  campos requeridos.
- Bad requests → 400 `ERR.VALIDATION`: falta un parámetro, la fecha es inválida, el JSON es inválido (también
  en `CreatePerson`). Un error real sigue en 500.

## Pendiente

- **Probar contra la base** (SQL Server en `localhost` no respondía en toda la sesión): todavía no se probó
  el SQL ni el multi-mapping con datos reales.
- Paginado de `GetPersons` (hay un `TODO` en el repo).
- Validación de los parámetros de las queries (el `TODO.md` del usuario menciona FluentValidation).
- Cambios del usuario sin commitear (no son parte de esta tarea): `TODO.md`, `Records.sln` (Visual Studio
  agregó `TODO.md` como Solution Item) y la documentación XML de `GetPersonByIdEndpoint.cs`.

## Próximos pasos concretos

1. Levantar SQL Server y probar:
   - `GET /persons/{id}`: con address y assets; sin address ni assets; id inexistente (404).
   - `GET /persons`: el orden por `FullName`, y que los assets queden agrupados en su persona.
   - `GET /persons/by-gender-and-birthdate?gender=Male&birthDateFrom=1980-01-01&birthDateTo=1999-12-31`.
   - `POST /persons/with-specific-profile` con `{"gender":"Male","ageOlderThan":30,"liveInCity":"Miami Beach"}`.
2. Para una query nueva: crear la query (`IQuery<TResponse>`) + handler en Application, un repo
   `QueryRepository<TQuery, TResponse>` con su SQL (columnas en el orden de `QueryPersonsAsync` si devuelve
   personas) y el endpoint. No hay que tocar el registro en DI.

## Gotchas

- **El orden de columnas del SELECT define el `splitOn`** (`"Id,Lat,Id"`): Person… | `AD.Id`… | `AD.Lat`,
  `AD.Lng` | `PA.Id`… Dapper devuelve `null` para un split si su **primera** columna es NULL: sin address
  (`AD.Id`), sin coordenadas (`AD.Lat`) o sin assets (`PA.Id`).
- **El repo de referencia no clona en Windows** ("Filename too long"). Los archivos se leen con
  `git ls-tree -r HEAD` + `git cat-file -p <hash>`, o habilitando `core.longpaths`.
- **`dotnet sln add` agrega configuraciones x64/x86 a todos los proyectos.** Se revirtió y el proyecto se
  agregó al `.sln` a mano (Project + 4 líneas de config + NestedProjects).
- **SA1402** marca un archivo con dos clases genéricas de distinta aridad. La convención del repo es
  `#pragma warning disable SA1402`, como en `RequestHandler.cs`. Ya no aplica a `QueryRepository.cs`
  (quedó un solo tipo).
- **En Development, minimal APIs lanza `BadHttpRequestException`** (`ThrowOnBadRequest`) en vez de devolver
  400. Por eso hubo que manejarla en `GlobalExceptionHandler`.
- **Visual Studio muestra errores viejos de "init-only property"** después de cambiar `init` → `set`. El
  build pasa; es caché del IDE.
- **Los heredocs de bash con comillas simples en el contenido** (p.ej. "It's") rompieron un comando que
  creaba varios archivos a la vez. Para archivos C#, mejor usar la herramienta Write.
