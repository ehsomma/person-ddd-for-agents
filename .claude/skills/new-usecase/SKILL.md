---
name: new-usecase
description: Usar SIEMPRE al agregar un caso de uso de punta a punta en este repositorio (command, query o handler de un domain event, con su endpoint), al agregar un aggregate nuevo a un módulo existente o al agregar o cambiar un campo que se persiste (cambio de esquema). También cuando se pide "agregá un endpoint para…", "un caso de uso para…", "que la API permita…" o "agregá el campo X a…". Define el orden de los pasos por capa, qué archivo de referencia imitar, las convenciones de nombre y de visibilidad de las que depende el registro automático en DI (Scrutor), los archivos que toca un cambio de esquema y la verificación final (build, convenciones y arranque de la API). Si el caso de uso necesita un bounded context que todavía no existe, primero se usa la skill `new-module`.
---

# Casos de uso nuevos

Es la tarea más frecuente en los proyectos que salen de esta plantilla, y es donde están las fallas que
**no avisa nadie**: el build pasa con 0 warnings y el problema aparece recién al arrancar la API o al
primer request. Por eso esta skill termina con una [verificación](#verificación-final) que no se
saltea.

En los ejemplos, `<Módulo>` es el bounded context (`Persons`), `<Entidad>` el aggregate root
(`Person`), `<UseCase>` el caso de uso (`DeletePerson`) y `<Evento>` el evento en pasado
(`PersonDeleted`). El archivo que hay que imitar en cada paso es el de la tabla
**Archivos de referencia** del `CLAUDE.md`.

## Antes de empezar

1. **Cargá las skills `team-standards` y `code-comments`.** Esta skill dice qué archivos crear y en qué
   orden. Cómo se escribe el código y los comentarios lo dicen esas dos.
2. **Clasificá el caso de uso:**

   | Si el caso de uso… | Es un… | Casos de este repo para imitar |
   |---|---|---|
   | Cambia el estado de un aggregate | Command | `DeletePerson` (sin resultado, 204), `UpdatePerson` y `CreatePerson` (devuelven el DTO) |
   | Solo lee | Query | `GetPersonById` (por ruta, 404 si no existe), `GetPersons` (sin parámetros), `GetPersonsByGenderAndBirthdate` (query string), `GetPersonsWithSpecificProfile` (POST con la query en el body) |
   | Reacciona a algo que ya pasó en el dominio | Event handler | `PersonDeletedEventHandler` |

3. **Detectá qué más implica:**
   - ¿El bounded context no existe? → primero la skill `new-module`.
   - ¿Hace falta un aggregate que no existe? → [Aggregate nuevo](#aggregate-nuevo-en-un-módulo-existente).
   - ¿Hay un campo nuevo o distinto que llega a la base? → [Cambio de esquema](#cambio-de-esquema).
4. **Si implica un aggregate nuevo, un cambio de esquema o un módulo nuevo, mostrale al usuario la lista
   de archivos a crear o tocar, con sus nombres, y esperá el OK.** Son decisiones de diseño, y el
   `CLAUDE.md` pide no inventar patrones sin consultar. Un command o una query sobre un aggregate que ya
   existe no necesita confirmación.

## Nombres, carpetas y visibilidad

| Pieza | Carpeta (proyecto `<Producto>.<Módulo>.*`) | Nombre | Visibilidad | Hereda o implementa |
|---|---|---|---|---|
| Command | `Application/<Entidad>/Commands/<UseCase>/` | `<UseCase>Command` | `public sealed` | `ICommand` o `ICommand<TResult>` |
| Command handler | ídem | `<UseCase>CommandHandler` | `internal sealed` | `CommandHandler<TCommand>` o `CommandHandler<TCommand, TResult>` |
| Query | `Application/<Entidad>/Queries/<UseCase>/` | `<UseCase>Query` | `public sealed` | `IQuery<TResult>` |
| Query handler | ídem | `<UseCase>QueryHandler` | `internal sealed` | `QueryHandler<TQuery, TResult>` |
| Query repository | `Infra.Queries.Sql/<Entidad>/<UseCase>/` | `<UseCase>Repository` | **`public`** `sealed` | `QueryRepository<TQuery, TResult>` |
| Domain event | `Domain/<Entidad>Aggregate/Events/` | `<Evento>Event` | `public sealed` | `DomainEvent` |
| Event handler | `Application/<Entidad>/Events/<Evento>/` | `<Evento>EventHandler` | `internal sealed` | `DomainEventHandler<<Evento>Event>` |
| Error | `Domain/<Entidad>Aggregate/Errors/DomainErrors.cs` | `DomainErrors.<Entidad>.<Error>`, código `ERR.<ENTIDAD>.<ERROR>` | `public static` | `Error` |
| Repositorio (write) | `Infra.Persistence.Sql/<Entidad>Aggregate/Repository/` | `<Entidad>Repository` | **`public`** | `Repository`, `I<Entidad>Repository` |
| Mapper de persistencia | `Infra.Persistence.Sql/<Entidad>Aggregate/Mappers/` | `<Entidad>Mapper` | `public` | `IPersistenceMapper<DomainModel, DataModel>` |
| Mapper de application | `Application/<Entidad>/Mappers/` | `<Entidad>Mapper` | `internal sealed` | `IDomainMapper<DomainModel, Dto>` |
| Servicio de dominio | `Domain/<Entidad>Aggregate/Services/` | `<Entidad>Service` + `I<Entidad>Service` | `public sealed` | `IDomainService` |
| Endpoint | `Api.V1/<Entidad>/<UseCase>/` (en `Src/Apps`) | `<UseCase>Endpoint`; el método y el `WithName` son `<Verbo><UseCase>` (`GetGetPersonById`, `PutUpdatePerson`) | `internal sealed`, el método **`public`** | `IEndpoint` |
| Validator | `Api.V1/<Entidad>/Validators/` | `<Dto>Validator` | `internal sealed` | `AbstractValidator<Dto>` |

El nombre del caso de uso es el mismo en la carpeta, el command o la query, el handler, el query
repository y el endpoint. Si el usuario pide un nombre distinto para el endpoint, avisale y usá uno solo.

## Registro en DI: lo que falla en silencio

No hay que tocar `Program.cs` para un caso de uso nuevo **dentro de un módulo que ya existe**: Scrutor
escanea los assemblies del módulo, que ya están registrados. Pero solo encuentra lo que respeta la
convención:

| Pieza | Se registra si… | Si no se cumple… |
|---|---|---|
| Repositorios (write y read) | El nombre termina en `Repository` **y la clase es `public`** (`AddPersistence` no escanea las `internal`). Se registra por sus interfaces. | El build pasa. En Development la API **no arranca** ("Some services are not able to be constructed"); en otro ambiente, 500 en el primer request. |
| Mappers | El nombre termina en `Mapper` (puede ser `internal`). | Igual que los repositorios. |
| Servicios de dominio | Implementa `IDomainService`. Se registra por su interfaz propia (`I<Entidad>Service`). | Igual que los repositorios. |
| Command y query handlers | Hereda de `CommandHandler` o `QueryHandler` y está en el proyecto Application del módulo. | 500 en el request: el mediator no encuentra `IRequestHandler<...>`. |
| Event handlers | Hereda de `DomainEventHandler<T>` con el tipo **exacto** del evento y está en el proyecto Application. | ⚠️ **Nada**: el evento se publica, nadie lo atiende y no hay error ni log. |
| Endpoints | Implementa `IEndpoint`. | La ruta no existe: 404 sin error. |
| Validators | Hereda de `AbstractValidator<T>` y está en el proyecto de la API. | 500 en el request, al resolver `IValidator<T>`. |

Además, **los tipos genéricos tienen que coincidir exactos** entre lo que inyecta el handler y lo que
implementa el repositorio: `IQueryRepository<GetPersonsQuery, IList<Dto.Person>>` no se resuelve con un
`QueryRepository<GetPersonsQuery, List<Dto.Person>>`. Las anotaciones de nulabilidad (`Dto.Person?`) no
cuentan, porque en runtime no existen.

## Command

1. **Dominio:**
   - El método de comportamiento en el aggregate (imitá `Person.Delete` o `Person.Update`): aplica las
     reglas de negocio, cambia el estado con los setters privados y registra el evento con
     `RegisterDomainEvent(...)`.
   - El evento, si corresponde (imitá `PersonDeletedEvent`): recibe el aggregate y le pasa su ID a la
     base (`base(aggregate.Id.ToString())`).
   - El error nuevo en `DomainErrors.<Entidad>`, con el código y el mensaje en inglés. El `ErrorType`
     define el status HTTP, que resuelve solo `GlobalExceptionHandler`:

     | `ErrorType` | Status |
     |---|---|
     | `Validation` | 400 |
     | `Unauthorized` | 401 |
     | `Forbidden` | 403 |
     | `NotFound` | 404 |
     | `Conflict` | 409 |
     | `Failure` | 500 |

   - Un método nuevo en `I<Entidad>Repository` solo si el command lo necesita. El repositorio es una
     colección de aggregates (`GetByIdAsync`, `InsertAsync`, `UpdateAsync`, `DeleteAsync`), no un lugar
     para consultas sueltas.
2. **Application:** el command y su handler (imitá `DeletePerson` si no devuelve nada o `UpdatePerson` si
   devuelve el DTO). El handler sigue siempre el flujo del `CLAUDE.md`: cargar el aggregate con el
   servicio de dominio (nunca un `Exists()`), llamar al método de comportamiento,
   `ExecuteInTransaction(...)` y `PublishDomainEvents(aggregate.PullDomainEvents())`.
   - Si devuelve el DTO, vuelve a leer el aggregate después del commit y lo mapea con el
     `IDomainMapper`, porque el modelo de dominio no recibe los IDs que genera el insert (ver
     `UpdatePersonCommandHandler`).
   - Si crea un aggregate, lo arma desde el DTO con los `Build(...)` de los value objects y el
     `Create(...)` del aggregate (ver `CreatePersonCommandHandler`).
3. **Infra (write):** solo si agregaste un método al repositorio. Imitá `PersonRepository`:
   - Dapper.Contrib no acepta `CancellationToken`: `cancellationToken.ThrowIfCancellationRequested()`
     antes de cada llamada.
   - El SQL propio va con `CommandDefinition` para pasarle el token.
   - Los inserts y deletes de hijos son secuenciales (no `Task.WhenAll`), y se borran primero los hijos
     por las FKs.
   - El `SELECT` que carga el aggregate para un command va **sin** `NOLOCK`.
4. **API:**
   - El validator, si hay body (imitá `PersonValidator`): las longitudes replican las de los value
     objects.
   - El endpoint (imitá `DeletePersonEndpoint` o `UpdatePersonEndpoint`), con el orden de parámetros, los
     atributos `[From...]` y los XML docs de `team-standards` y `code-comments`. Declará con
     `.Produces<ErrorResponse>(...)` y su `<response code>` solo los códigos propios del endpoint (por
     ejemplo el 404). El 500 y el 400 del body los agrega `MapEndpoints`.
   - Si la operación queda con una sola respuesta documentada, agregá un `<response code="...">`
     explícito para el caso de éxito (si no, Scalar usa el `<returns>` como descripción).

## Query

Las queries no pasan por el dominio: el query repository mapea directo al DTO.

1. **Application:** la query (`IQuery<TResult>`) y su handler, que inyecta
   `IQueryRepository<TQuery, TResult>` directo (sin una interfaz propia del repo). La query existe
   siempre, aunque no tenga parámetros (`GetPersonsQuery`), porque el mediator resuelve el handler por
   su tipo.
   - Si devuelve un solo recurso y el repo devuelve `null`, el handler lanza
     `new DomainException(DomainErrors.<Entidad>.NotFound)`: el mismo 404 que los commands.
   - Si devuelve una lista, una lista vacía no es un error.
2. **Infra (read):** `<UseCase>Repository : QueryRepository<TQuery, TResult>` (imitá
   `GetPersonByIdRepository`):
   - `WITH (NOLOCK)` en **todas** las tablas del `SELECT`, incluidas las de los `JOIN`.
   - `CommandDefinition` con el `cancellationToken` y `await using DbConnection connection = CreateConnection();`
     (las queries no usan la `IDbSession` de los commands).
   - Para una columna `varchar`, el parámetro va como `DbString { IsAnsi = true, Length = n }`. Un
     `string` se manda como `nvarchar`, convierte la columna e impide usar el índice.
   - ⚠️ Si devuelve el aggregate con sus hijos, reusá el multi-mapping del módulo (en Persons,
     `PersonMultiMappingExtensions.QueryPersonsAsync`) y respetá **el orden de columnas** que documenta
     su `<remarks>`. Ese orden define el `splitOn`: con una columna corrida, Dapper parte la fila en el
     lugar equivocado y mapea datos cruzados o `null` sin dar error.
3. **API:** el endpoint (imitá `GetPersonByIdEndpoint`). Los filtros simples van con `[FromQuery]`. Si el
   filtro es complejo, la query llega como `[FromBody]` en un POST (imitá
   `GetPersonsWithSpecificProfile`).

## Event handler

1. Confirmá que el evento existe y que algún método del aggregate lo registra. Si no, el evento va
   primero (ver [Command](#command), paso 1).
2. El handler va en `Application/<Entidad>/Events/<Evento>/` (imitá `PersonDeletedEventHandler`, que
   también documenta qué se puede hacer desde un event handler y qué no).
3. Se ejecuta **después del commit**: lo que hizo el command ya está persistido, y no se puede cancelar.
   Los handlers de un mismo evento corren en secuencia y comparten las dependencias scoped del request.

## Aggregate nuevo en un módulo existente

Imitá cada pieza del aggregate `Person`:

| Capa | Archivos |
|---|---|
| Domain | `Models/<Entidad>.cs` (`AggregateRoot<Guid>`, constructor privado, `Create` que registra `<Entidad>CreatedEvent`, `Load` sin eventos, setters privados), sus `ValueObjects/`, `Errors/DomainErrors.cs` (o una clase anidada nueva), `Events/`, `Repositories/I<Entidad>Repository.cs`, `Services/I<Entidad>Service.cs` y `<Entidad>Service.cs` (su `GetByIdAsync` lanza el `NotFound`). |
| Dtos | `<Entidad>/<Entidad>.cs`, con `<example>` en cada propiedad. |
| Application | `<Entidad>/Mappers/<Entidad>Mapper.cs` (`IDomainMapper`). |
| Infra.Persistence.Sql | `<Entidad>Aggregate/Models/` (data models), `Mappers/<Entidad>Mapper.cs` y `Repository/<Entidad>Repository.cs`. |
| Base | La tabla en el script SQL del módulo. |

- **IDs:** `Guid` en el aggregate root (`[ExplicitKey]` en el data model). Las entidades hijas usan
  `int` identity (`[Key]`), y `0` significa nueva.
- **Navegaciones del data model:** las propiedades que apuntan a otras tablas llevan `[Write(false)]`,
  para que Dapper.Contrib no intente escribirlas como columnas.
- **Auditoría:** si la tabla tiene `CreatedOnUtc` y `UpdatedOnUtc`, el aggregate implementa
  `IEntityAuditable` y el data model hereda de `EntityAuditable`.

Como todo queda dentro de los proyectos del módulo, no hay que tocar `Program.cs`.

## Cambio de esquema

Agregar, renombrar o cambiar un campo que se persiste toca **muchas capas**, y ninguna falla si te
olvidás de una: el campo simplemente no se guarda, no se lee o llega en `null`. Recorré la lista
completa:

1. **Script SQL** del módulo (`<Producto><Módulo>_Source.sql`, en Persons `RecordsPersons_Source.sql`):
   la columna, con su tipo y su largo.
2. **Value object**, si el campo tiene reglas (largo, formato): con constructor privado y `Build(...)`.
3. **Aggregate o entidad:** la propiedad con setter privado, y el parámetro en el constructor, `Create`,
   `Load` y el método de comportamiento que lo modifica (`Update`). Si es obligatorio, va también en la
   validación de datos mínimos (`EnsureMinimalRequiredData`).
4. **Data model** (`Infra.Persistence.Sql/<Entidad>Aggregate/Models/`): la propiedad, con el mismo
   nombre que la columna. Los `InsertAsync` y `UpdateAsync` de Dapper.Contrib la toman solos.
5. **Mapper de persistencia, en los dos sentidos:** `FromDomainToDataModel` y `FromDataModelToDomain`.
6. **El `SELECT` de `GetByIdAsync` del repositorio (write):** tiene la lista de columnas explícita. Si
   no agregás la columna ahí, el aggregate se carga sin el dato y el próximo `Update` lo pisa con
   `null`. ⚠️
7. **DTO** (`Dtos/<Entidad>/`): la propiedad, con su `<summary>` y su `<example>`.
8. **Mapper de application** (`IDomainMapper`, dominio → DTO).
9. **Los command handlers que arman el dominio desde el DTO** (en Persons, `CreatePersonCommandHandler`
   y `UpdatePersonCommandHandler.LoadPerson`).
10. **Validator** de la API: la regla, con el mismo largo que el value object o la columna.
11. **El SQL de los query repositories** que leen esa tabla, y el multi-mapping si lo usan. La columna
    va dentro del tramo de su objeto, antes de la columna en la que arranca el siguiente split. Si el
    orden cambia, actualizá el `<remarks>` del multi-mapping y el `splitOn`.

Para encontrar todos los lugares, buscá una columna hermana que ya exista y que recorra el mismo camino:

```bash
grep -rn "Birthdate" Src/Modules/<Módulo> Src/Apps *.sql
```

Cada archivo que aparece con la hermana tiene que aparecer también con el campo nuevo. Hacé la misma
búsqueda con el campo nuevo al terminar y compará las dos listas.

## Verificación final

No cierres la tarea sin hacer estos pasos y sin reportar el resultado de cada uno.

1. **Build:** `dotnet build Records.slnx` con 0 warnings (incluye las reglas SA, CA, IDE y ARCH). Si
   aparece un ARCH002, se corrige la referencia, nunca la validación.
2. **Convenciones de DI.** Cada comando tiene que devolver **nada**:

   ```bash
   # Repositorios que no son públicos (Scrutor no los registra).
   grep -rnE "^\s*(internal\s+)?(sealed\s+)?class\s+\w+Repository\b" Src/Modules --include=*.cs

   # Clases que heredan de un repositorio y no terminan en Repository.
   grep -rnE "class\s+\w+\s*:\s*.*(QueryRepository<|Persistence\.Repository\b)" Src/Modules --include=*.cs | grep -vE "class\s+\w+Repository\b"

   # Mappers que no terminan en Mapper.
   grep -rnE "class\s+\w+\s*:\s*.*I(Domain|Persistence)Mapper<" Src --include=*.cs | grep -vE "class\s+\w+Mapper\b"

   # Handlers que no siguen el nombre del caso de uso.
   grep -rnE "class\s+\w+\s*:\s*(CommandHandler|QueryHandler|DomainEventHandler)<" Src/Modules --include=*.cs | grep -vE "class\s+\w+(CommandHandler|QueryHandler|EventHandler)\b"
   ```

   Si agregaste un handler, confirmá también a mano que está en la carpeta de su caso de uso, que el
   nombre coincide con el del command, la query o el evento, y que los tipos genéricos del
   `IQueryRepository<,>` que inyecta son exactamente los del repositorio.
3. **Arranque de la API.** En Development, el contenedor de DI valida todo el grafo al arrancar, así que
   un repositorio, un mapper o un servicio sin registrar hace fallar el arranque, sin necesidad de SQL
   Server. Levantala en background con el perfil `http` (Development, en el puerto de su
   `applicationUrl`):

   ```bash
   dotnet run --project Src/Apps/<Host> --launch-profile http
   ```

   - ⚠️ Si la consola dice `The launch profile "http" could not be applied`, la API arrancó igual pero
     sin el perfil: en Production (sin la validación de DI) y en otro puerto. Casi siempre es un
     comentario en el `launchSettings.json`: Visual Studio los acepta y `dotnet run` no. Sacalo y
     volvé a arrancar.
   - Si en la consola aparece "Some services are not able to be constructed", falta registrar algo: el
     mensaje dice qué tipo no se pudo resolver.
   - Con la API arriba, pedí `<applicationUrl>/openapi/v1.json` (en Persons,
     `http://localhost:5133/openapi/v1.json`) y confirmá que está la ruta nueva,
     con su summary y sus responses. Si la ruta no está, el endpoint no implementa `IEndpoint`. Si está
     sin descripciones, el método del endpoint no es `public`.
   - Frená el proceso al terminar.
4. **Lo que esta verificación no cubre:** un event handler mal tipado no da ningún error (revisalo a
   mano, ver el paso 2), y el SQL no se prueba sin la base. Si no pudiste probar contra la base, decilo
   en el reporte en lugar de darlo por probado.

## Checklist antes de cerrar

- [ ] El caso de uso tiene el mismo nombre en la carpeta, el command o la query, el handler, el query
      repository y el endpoint.
- [ ] El command carga el aggregate completo, llama al método de comportamiento, persiste con
      `ExecuteInTransaction` y publica los eventos después del commit, sin `CancellationToken`.
- [ ] Los errores nuevos están en `DomainErrors`, con código, mensaje en inglés y `ErrorType`.
- [ ] Todos los `SELECT` de las queries llevan `WITH (NOLOCK)`, y el que carga un aggregate para un
      command no.
- [ ] El `CancellationToken` llega hasta el repositorio.
- [ ] Si hubo un cambio de esquema, la búsqueda de la columna hermana y la del campo nuevo devuelven los
      mismos archivos.
- [ ] El build da 0 warnings, los `grep` de convenciones no devuelven nada y la API arranca con la ruta
      nueva en `/openapi/v1.json`.
- [ ] El reporte dice qué se verificó y qué no (por ejemplo, el SQL contra la base).
