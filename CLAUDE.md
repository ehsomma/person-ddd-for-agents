# CLAUDE.md

Proyecto modelo base en C# sobre .NET 10, con Domain-Driven Design (DDD) y CQRS, estructurado con Clean Architecture
y con estándares de codificación, naming, arquitectura de directorios y documentación de código.

**Este repo es la referencia que usan los agentes de AI para generar código en otros proyectos.** Antes de escribir
algo nuevo, buscá el caso existente más parecido (ver [Archivos de referencia](#archivos-de-referencia)) e imitá su
estructura, naming y documentación. No inventes patrones nuevos sin consultar.

## Comandos

```bash
dotnet build Records.slnx                               # objetivo: 0 warnings (incluye las reglas IDE del .editorconfig)
dotnet build Records.slnx -p:CI=true                    # igual que el CI: TreatWarningsAsErrors, cualquier warning rompe el build
dotnet run --project Src/Apps/Records.Persons.Api.V1    # Scalar en /v1/docs, requiere SQL Server
dotnet test --solution Records.slnx                     # corre con Microsoft Testing Platform (global.json), lleva --solution
./Tests/coverage.ps1 -Assemblies "+Records.Persons.Domain" -Open   # cobertura local, todo en Tests/TestResults
```

- El esquema de la base está en `RecordsPersons_Source.sql`.
- Los tests están en `Tests/`, con la misma estructura de carpetas que `Src/`: xUnit v3, AwesomeAssertions y Moq.
  Hoy hay tests del dominio de Persons y de `BuildingBlocks.Domain`.
- **Los tests nuevos se escriben solo a pedido**, con el código ya consensuado, pero los existentes se corren siempre
  que se cambia `Src/`. **La cobertura se corre solo cuando el usuario la pide** (ReportGenerator). Cómo se escriben
  los tests y cómo se interpreta la cobertura está en la skill `unit-tests`.

## Arquitectura

| Carpeta | Qué contiene |
|---|---|
| `Src/Core/My.*` | Utilidades transversales sin dependencias del dominio (`My.Exceptions` con `ErrorType`, extensiones, conexión interceptable). |
| `Src/BuildingBlocks/*` | Base reutilizable entre módulos: clases base de dominio, CQRS, mediator propio, persistencia, HTTP, OpenAPI, Serilog. |
| `Src/Modules/<Módulo>/Records.<Módulo>.*` | Un bounded context: `Domain`, `Application`, `Dtos`, `Configuration`, `Infra.Persistence.Sql` (write side), `Infra.Queries.Sql` (read side) e `Infra.Configuration.DependencyInjection`. |
| `Src/Apps/*` | Hosts (APIs). Solo ellos referencian los proyectos `*.DependencyInjection`. |

- **Las dependencias entre capas las valida `Directory.Build.targets` en cada build.** La capa se infiere del nombre del
  proyecto. Si no se puede inferir sale `ARCH001` y hay que declarar `<ArchLayer>`. Una referencia prohibida da
  `ARCH002`. Nunca apagues la validación (`ArchValidationEnabled=false`) para sacarte un ARCH002 de encima: lo que
  hay que corregir es la referencia.
- **Write side:** Dapper sobre data models con un mapper manual dominio ↔ data model. Las operaciones corren en una
  transacción del `IUnitOfWork`.
- **Read side:** las queries leen con `QueryRepository<TQuery, TResult>` y Dapper mapea directo a DTOs, sin pasar por el
  dominio. Todo `SELECT` lleva `WITH (NOLOCK)`, salvo los que cargan un agregado para un command.

## Decisiones de diseño

Estas son cosas que un agente suele hacer por costumbre y que **acá no se hacen**:

- **Mediator propio** (`BuildingBlocks.Mediator*`): no se usa MediatR.
- **Persistencia con Dapper**: no se usa EF Core.
- **Mappers manuales** (`IDomainMapper<,>`, `IPersistenceMapper<,>`): no se usa AutoMapper ni Mapster.
- **Los errores son excepciones**, no se usa el patrón Result.
  - Se lanza `new DomainException(DomainErrors.<Entidad>.<Error>)`, y cada `Error` lleva código, mensaje y `ErrorType`.
  - El status HTTP lo resuelve **solo** `GlobalExceptionHandler` a partir del `ErrorType`.
  - La respuesta de error es siempre `ErrorResponse` (`BuildingBlocks.Contracts`), nunca ProblemDetails.
- **El origen de una operación no es un parámetro genérico de los commands.** Si el negocio lo usa (p.ej. el canal de
  una reserva), es un dato del command que viaja en el body. Si es identidad técnica del cliente, sale de la
  autenticación (`client_id`) cuando exista. No se crea un contexto del llamador (`ICallerContext`, `ICurrentUser`)
  hasta que exista un caso de uso que lo consuma.
- **IDs:** `Guid` en los agregados, sin strongly-typed IDs. Las entidades hijas usan `int`, y `0` significa nueva.
- **Validación en dos niveles:** FluentValidation en la API y las invariantes en el dominio. Los value objects tienen
  constructor privado y se crean con `static Build(...)`, que valida con la librería `Throw`.
- **Agregados:** constructor privado, `static Create(...)` (registra el evento de creación) y `static Load(...)`
  (rehidrata sin eventos). El comportamiento va en métodos del agregado, y los setters son privados.
- **Los commands cargan el agregado completo**, no hacen un chequeo con `Exists()`. El flujo es:
  1. Cargar el agregado con el servicio de dominio.
  2. Llamar al método de comportamiento.
  3. `ExecuteInTransaction(...)`.
  4. `PublishDomainEvents(aggregate.PullDomainEvents())`.

  Los eventos de dominio se publican **después del commit**, y `PublishDomainEvents` no recibe `CancellationToken`:
  lo ya commiteado no se cancela.
- **`CancellationToken` hasta el repositorio en los dos lados** (read y write). Dapper.Contrib no lo acepta, así que
  se chequea con `ThrowIfCancellationRequested()` antes de cada llamada.

## Archivos de referencia

Las rutas son relativas a `Src/Modules/Persons/` salvo que se indique otra cosa.

| Para crear… | Imitar |
|---|---|
| Agregado | `Records.Persons.Domain/PersonAggregate/Models/Person.cs` |
| Value object | `Records.Persons.Domain/PersonAggregate/ValueObjects/FullName.cs` |
| Error de dominio | `Records.Persons.Domain/PersonAggregate/Errors/DomainErrors.cs` |
| Evento de dominio | `Records.Persons.Domain/PersonAggregate/Events/PersonDeletedEvent.cs` |
| Command + handler | `Records.Persons.Application/Person/Commands/DeletePerson/*` |
| Query + handler | `Records.Persons.Application/Person/Queries/GetPersonById/*` |
| Query repository (read) | `Records.Persons.Infra.Queries.Sql/Person/GetPersonById/GetPersonByIdRepository.cs` |
| Event handler | `Records.Persons.Application/Person/Events/PersonDeleted/PersonDeletedEventHandler.cs` |
| Repositorio (write) + mapper | `Records.Persons.Domain/PersonAggregate/Repositories/IPersonRepository.cs` (contrato) y `Records.Persons.Infra.Persistence.Sql/PersonAggregate/{Repository,Mappers}/*` |
| DTO | `Records.Persons.Dtos/Person/Person.cs` |
| Endpoint | `Src/Apps/Records.Persons.Api.V1/Person/DeletePerson/DeletePersonEndpoint.cs` |
| Validator | `Src/Apps/Records.Persons.Api.V1/Person/Validators/PersonValidator.cs` |
| Cableado de DI | `Src/Apps/Records.Persons.Api.V1/Program.cs` |

### Casos de uso y módulos nuevos

Las recetas están en dos skills, que se cargan solo cuando hacen falta:

- **`new-usecase`**: un command, una query o un event handler de punta a punta, un aggregate nuevo en un módulo
  existente o un cambio de esquema. Termina con una verificación (build, convenciones de nombre, tests existentes y
  arranque de la API) que no se saltea.
- **`new-module`**: un bounded context nuevo, su host y un proyecto nuevo a partir de esta plantilla.

**El registro en DI es automático por convención de nombres** (Scrutor): una clase que no la respeta no se
registra y el build no avisa. El detalle está en `new-usecase`.

## Cómo se aplican las reglas de código

1. Antes de escribir C#, se leen las skills `team-standards` y `code-comments` y el archivo de referencia del caso
   (ver arriba). Del archivo de referencia se imitan la estructura y el código. En los comentarios, si el archivo
   contradice a `code-comments`, gana la skill.
2. El build es el juez de todo lo que tiene analyzer: SA (StyleCop), CA (`AnalysisMode=All`), IDE (`.editorconfig`,
   por `EnforceCodeStyleInBuild`) y ARCH (`Directory.Build.targets`). VS, `dotnet build` y el CI ven las mismas reglas.
3. Las reglas en `none`, `silent` o `suggestion` no las verifica el build: dejan libertad, salvo que la skill pida una
   forma concreta.
4. Si la skill y un analyzer se contradicen, no se elige uno en silencio: se frena, se avisa y se corrige la skill o
   el `.editorconfig`.

## Convenciones

- **Carpetas:** `<Entidad>/{Commands,Queries,Events,Mappers}/<UseCase>/<UseCase>{Command,CommandHandler,Query,QueryHandler}.cs`.
- **Visibilidad:** los handlers, endpoints y validators son `internal sealed`. Los commands y queries son `public sealed`.
- **Idioma** (el detalle, el tono y el formato de los comentarios están en la skill `code-comments`):
  - En inglés: los identificadores, los mensajes y códigos de error (`ERR.PERSON.NOTFOUND`), los mensajes de log y
    los XML docs de `Src/Core` y `Src/BuildingBlocks`.
  - En español (es-AR): los XML docs de `Src/Modules` y `Src/Apps`, y en todo el repo los comentarios inline
    (`//`, `NOTE:`, `TODO:`) y los `Justification` de los `SuppressMessage`.
  - Los comentarios que ya existen no se traducen salvo que se pida explícitamente.
- **Estilo:** lo controlan `.editorconfig` y StyleCop con `AnalysisMode=All`. **Usá las skills `team-standards` y
  `code-comments`** al escribir o revisar C#: ahí están las reglas que ningún analyzer cubre (regiones, `return`,
  aliases, endpoints, idioma y formato de los comentarios, etc.).
- **`.editorconfig`:**
  - Todo override de severidad lleva un comentario con el código y el título de la regla, por ejemplo
    `dotnet_diagnostic.IDE0290.severity = silent # IDE0290: Use primary constructor`.
  - Las opciones de code style van sin sufijo de severidad (`= true`, nunca `= true:silent`). VS y el build resuelven
    el sufijo distinto: en VS gana `category-Style = warning` y en el build gana el sufijo, así que la misma regla
    quedaría warning en VS y silent en el build. Para cambiar la severidad de una regla IDE se usa solo
    `dotnet_diagnostic.IDExxxx.severity`. Ojo: hay reglas con el mismo título (IDE0028 e IDE0306), así que verificá
    el ID exacto.

## Trampas conocidas

- **La solución es `Records.slnx`, no hay `.sln`:** los proyectos se agregan con
  `dotnet sln Records.slnx add <ruta>.csproj --solution-folder <carpeta>`, usando la misma carpeta que sus hermanos
  (por ejemplo `Src/Modules/Persons/Infra`). No crees un `.sln` nuevo: con dos archivos de solución en la raíz,
  `dotnet build` sin argumento falla con MSB1011.
- **El método del endpoint tiene que ser `public`**, aunque la clase sea `internal`. El generador de XML docs de OpenAPI
  ignora los métodos privados sin avisar. Al ser público, SA1615 exige un `<returns>`. Si la operación queda con una
  sola respuesta, el generador usa ese `<returns>` como descripción, así que agregá un `<response code="...">`
  explícito.
- **`AddOpenApi("v1", ...)` va literal en el `Program.cs` del host**, nunca dentro de un helper compartido. El source
  generator intercepta ese call site, necesita que el nombre del documento sea un literal y resuelve los XML docs
  según los `ProjectReference` de ese proyecto. Si no se cumple, Scalar se queda sin descripciones ni ejemplos y no
  hay ningún error. Para diagnosticarlo, mirá el `/openapi/v1.json` crudo, no la UI.
- **`ValidateAndThrowExeption` se escribe así a propósito** (es el nombre real del método). No lo corrijas de pasada.
- **Errores en el IDE con la build limpia:** si se renombró algo desde la shell, es la caché de ReSharper. Primero mirá
  si el Error List de VS está vacío. Si lo está, la solución es ReSharper → Options → Environment → General → Clear
  caches, y reiniciar VS. Borrar `.vs` no alcanza.

## Flujo de trabajo

- **Commits:** siempre con la skill `git-commit`. Son Conventional Commits en español, se muestra el mensaje y se
  espera confirmación antes de ejecutar, y no llevan trailers.
- **Handoffs:** van en `Docs/handoff/` y **solo se crean o actualizan cuando el usuario corre `/handoff`**. No los
  actualices por tu cuenta ni los pongas como paso de un plan. Para retomar una tarea se usa `/handoff-resume <nombre>`.
- Antes de tocar un área, leé su handoff. Ahí están las decisiones y el porqué.

| Handoff | Qué decisión registra |
|---|---|
| `architecture-build-validation` | La matriz de capas validada por MSBuild y la ubicación de las abstracciones de persistencia. |
| `person-repository` | Dapper con data models y mapper manual, y `ExecuteInTransaction`. |
| `queries` | El read side: query repositories que devuelven DTOs y `NOLOCK`. |
| `usecase-delete-person` | Cargar el agregado completo, y el flujo de un command con eventos. |
| `domain-event-handlers` | `DomainEventHandler<T>` y `DomainMessage<T>`. |
| `fluentvalidation-on-api` | La validación en la API y en el dominio, y el `ErrorResponse` único. |
| `mapeo-errores-statuscode` | El `ErrorType` como única fuente del status HTTP. |
| `api-xml-comments` | Los XML docs de los endpoints en Scalar y las respuestas comunes. |
| `editorconfig-vs-teamStandars` | Las reglas IDE en el build (`EnforceCodeStyleInBuild`), sin sufijos de severidad, y el skill alineado con el `.editorconfig`. |
| `cancelationtoken-solo-hasta-commit` | El `CancellationToken` hasta el repositorio en el write side, respetado solo hasta el commit, y los eventos sin token. |
| `connection-refactor` | La conexión de `DbSession` abierta de forma lazy y async, y el `IUnitOfWork` async (SqlClient no tiene transacciones async reales). |
| `domain-events-sin-mediatr` | **Obsoleto**: el diseño inicial, reemplazado por `BuildingBlocks.Mediator`. |

## Usar este repo como plantilla en otro proyecto

Los pasos (qué se copia, qué no, el prefijo, el host y el `CLAUDE.md` nuevo) están en la skill `new-module`, en la
sección "Proyecto nuevo a partir de esta plantilla".
