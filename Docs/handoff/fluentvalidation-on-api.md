# Handoff: fluentvalidation-on-api

## Objetivo

Validar los requests de `CreatePerson` y `UpdatePerson` con FluentValidation, tomando como base el
`CreatePersonController` del repo de referencia
([`ehsomma/ddd-cqrs-microservices`](https://github.com/ehsomma/ddd-cqrs-microservices)), y aclarar cómo convive
con la validación del dominio y con el `GlobalExceptionHandler`. En el camino se unificaron los errores de la API
para que todos respondan con el mismo `ErrorResponse`, en cualquier ambiente.

## Qué se hizo

Todo en `feature/mediator`, **sin commitear**.

### Archivos

| Archivo | Cambio | Por qué |
|---|---|---|
| `Records.Persons.Api.V1/Person/Validators/PersonValidator.cs` (nuevo) | Reglas de `Dto.Person`: `Id` no vacío, `FullName` (≤62), `Email` (≤362 + `EmailAddress()`), `Phone` (≤60), `Gender` contra `Gender.List` con mensaje propio, `Address` obligatorio y `PersonalAssets` con sus validators. | Las longitudes son las de los value objects del dominio. `Gender` usa la enumeración del dominio para no duplicar la lista de valores. |
| `.../Validators/AddressValidator.cs` (nuevo) | `StreetLine1` obligatorio (≤60); `StreetLine2`, `City` y `State` con `Length(1, n)`; `Country` (≤160); `LatLng` con su validator. | Los validadores de longitud de FluentValidation dan por válido el `null`: replica a `StringValueObjectNullable` (si viene un valor, mínimo 1 caracter). |
| `.../Validators/LatLngValidator.cs` (nuevo) | `Lat` obligatorio entre -90 y 90; `Lng` obligatorio entre -180 y 180. | Igual que el VO `LatLng`. |
| `.../Validators/PersonalAssetValidator.cs` (nuevo) | `Description` obligatorio (≤255); `Value > 0` con un `TODO`. | `Value > 0` viene del repo de referencia, pero **el dominio no lo valida** (`Money` acepta negativos). |
| `BuildingBlocks.Infra.Http.DependencyInjection/ServiceCollectionExtensions.cs` | `AddValidators(params Assembly[])`: registra los `IValidator<>` con Scrutor (`publicOnly: false`, scoped) y fija `ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en")`. | Scrutor ya estaba (lo usa `AddEndpoints`), así no hace falta el paquete `FluentValidation.DependencyInjectionExtensions`. Sin la cultura fija, los mensajes salían en el idioma del servidor (en esta PC, español). |
| `Records.Persons.Api.V1/Program.cs` | `services.AddValidators(Assembly.GetExecutingAssembly())` y `services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true)`. | Ver gotchas: sin `ThrowOnBadRequest`, fuera de Development los requests mal formados devolvían un 400 con body vacío. |
| `.../CreatePerson/CreatePersonEndpoint.cs` | Inyecta `IValidator<Dto.Person>` y llama a `personValidator.ValidateAndThrowExeption(person)` antes del `Send`. | Mismo patrón que el controller del repo de referencia. |
| `.../UpdatePerson/UpdatePersonEndpoint.cs` | Ídem. El chequeo de ID distinto entre ruta y body ahora lanza `My.Exceptions.ValidationException` con un `ValidationError` sobre `Id` (antes devolvía `TypedResults.Problem`). Retorno: `Task<Dto.Person>` (antes `Results<Ok<Dto.Person>, ProblemHttpResult>`). | Antes era el único error con formato `ProblemDetails` en vez de `ErrorResponse`. |
| `BuildingBlocks.Infra.Http/GlobalExceptionHandler.cs` | En el caso de `BadHttpRequestException`: devuelve `"The request is malformed or incomplete."` y loguea el mensaje original como warning (`Bad request: ...`). Se actualizó el `NOTE`. | El mensaje del framework está pensado para el programador ("...Did you mean to use a Service instead?") y expone nombres de parámetros internos. |
| `.editorconfig` | Sección `[*Validator.cs]` con `CA1812 = none` (comentada). | Los validators raíz se instancian por reflection, igual que Mappers y Handlers. |

### Decisiones (discutidas con el usuario)

- **Se valida en los dos lugares, cada uno con su rol.** FluentValidation valida el formato del request y
  devuelve **todos** los errores juntos y por campo. El dominio protege las invariantes (última línea de
  defensa). Con solo el dominio: corta en el primer error, los mensajes son técnicos (librería Throw),
  `ValidationErrors` llega en `null` (`DomainValidationException` usa el constructor que recibe un mensaje) y
  los nombres de campo son los del dominio (`Country.Name`) y no la ruta en el request (`Address.Country`).
  Las reglas de negocio que dependen de datos guardados (p.ej. "el país existe") quedan solo en el dominio.
- **Si FluentValidation deja pasar algo y lo frena el dominio:** 400 `ERR.VALIDATION`, con el mensaje técnico
  de Throw en `message` y `validationErrors: null`. Caso real encontrado: `john_doe@gmail.com` pasa
  `EmailAddress()`, pero el regex de `Email` del dominio no acepta `_`.
- **Usar `ValidateAndThrowExeption` (la nuestra), no `ValidateAndThrow()` de FluentValidation.** La nuestra
  lanza `My.Exceptions.ValidationException`, que el handler mapea a `validationErrors`. La de FluentValidation
  lanza `FluentValidation.ValidationException`, que el handler no reconoce, y terminaría en 500.
- **Un solo `PersonValidator` para Create y Update.** Hoy las reglas son idénticas, y dos
  `IValidator<Dto.Person>` registrados chocarían en el DI.
- **Sin `RuleFor(x => x.Person).NotNull()`** (sí está en el repo de referencia). Allá el body es un
  `CreatePersonRequest` que envuelve a la persona; acá el endpoint recibe `Dto.Person` directo, y si el body
  es vacío o `null`, ASP.NET lo rechaza antes del validator (parámetro no nullable = obligatorio).
- **Cultura de FluentValidation fijada dentro de `AddValidators`** (no en `Program.cs`), para que cualquier
  API que registre sus validators la tenga. Es una configuración global del proceso. El usuario puede preferir
  moverla a cada `Program.cs`.
- **En UpdatePerson, el chequeo de ID va antes que FluentValidation.** Si los IDs no coinciden, se responde solo
  ese error. Para devolverlo junto con los demás errores del body habría que tocar `ValidateAndThrowExeption`.
- **`ThrowOnBadRequest = true` en todos los ambientes + mensaje genérico en el handler**, en vez de
  `[FromBody]` en cada endpoint. Cubre todos los endpoints de una vez y no expone nombres internos.

## Verificación hecha

- Build de la solución: 0 errores, 0 warnings.
- Con la API levantada (sin SQL Server):
  - Request con 10 errores → 400 con los 10 en `validationErrors`, en inglés y con rutas anidadas
    (`Address.LatLng.Lat`, `PersonalAssets[0].Value`).
  - PUT con ID distinto entre ruta y body → 400 `ERR.VALIDATION` con el error sobre `Id`.
  - **En Production** (`ASPNETCORE_ENVIRONMENT=Production`): body vacío, `null`, JSON inválido y header
    `X-App-Key` faltante → 400 con `ErrorResponse` (antes: 400 con body vacío). El mensaje original queda en el
    log.
- Mensajes del dominio: verificados con un script aparte (`dotnet run t.cs` con `#:project` al proyecto de
  dominio) porque por HTTP el request no llega al handler sin base (ver gotchas).

## Pendiente

- **Commitear** (usar el skill `git-commit`). Sugerencia de partición: validators + DI + endpoints + editorconfig
  (`feat`); ID de UpdatePerson (`refactor`/`fix`); `ThrowOnBadRequest` + mensaje genérico (`fix(http)`).
- **`Value > 0` de PersonalAsset existe solo en la API** (`TODO` en `PersonalAssetValidator`). Si es regla de
  negocio, moverla a `Money`/`PersonalAsset`.
- **Gap entre FluentValidation y el dominio en `Email`:** el regex del dominio
  (`^(?!.*@.*@)[a-zA-Z0-9-.]+@+[a-zA-Z0-9].*`) rechaza `_`, `+`, etc. Decidir cuál es la regla correcta y
  alinear las dos (probablemente relajar el regex del dominio).
- **Mensaje genérico para bad requests:** el cliente no se entera de qué estuvo mal (p.ej. qué header falta).
  Opción propuesta y no implementada: mensajes por caso (body faltante, JSON inválido, header faltante) sin
  nombres internos.
- **Validación de los parámetros de las queries** (`GetPersonsByGenderAndBirthdate`,
  `GetPersonsWithSpecificProfile`): no se tocó.
- **OpenAPI de UpdatePerson:** al sacar `ProblemHttpResult` del tipo de retorno, el documento ya no muestra la
  respuesta 400 inferida (queda solo el `<response code="400">` del XML, igual que en CreatePerson).
- `TODO.md` del usuario: el ítem "Validación con fluentvalidation de endpoints" se puede marcar cuando se
  commitee.

## Próximos pasos concretos

1. Commitear con el skill `git-commit` (mostrar los mensajes y esperar confirmación).
2. Decidir con el usuario `Value > 0` y el regex de `Email`, y moverlos/alinearlos en el dominio.
3. Para validar un endpoint nuevo: crear `XxxValidator : AbstractValidator<TDto>` (`internal sealed`, nombre
   terminado en `Validator`) en el proyecto de la API, inyectar `IValidator<TDto>` en el handler del endpoint y
   llamar a `ValidateAndThrowExeption`. No hay que tocar el registro en DI.
4. Levantar SQL Server y probar el camino "lo frena el dominio" por HTTP (p.ej. email `john_doe@gmail.com`).

## Gotchas

- **Minimal APIs solo lanza `BadHttpRequestException` en Development** (`RouteHandlerOptions.ThrowOnBadRequest`).
  En el resto de los ambientes escribe un 400 vacío y la excepción nunca llega al `GlobalExceptionHandler`. Se
  probó en Production. Ahora está forzado a `true` en `Program.cs`; cada API nueva tiene que hacer lo mismo.
- **"Did you mean to use a Service instead?"** es parte del mensaje de ASP.NET cuando un parámetro de body
  *inferido* (sin `[FromBody]`) no llega: el framework no sabe si el cliente no mandó el body o si el
  programador quiso inyectar un servicio no registrado. Ahora solo aparece en el log.
- **Sin SQL Server, cualquier request válido termina en 500**: `DbSession` abre la conexión **en su constructor**,
  así que falla al resolver el handler, antes de llegar al dominio. Por eso los mensajes del dominio se probaron
  con un script aparte.
- **`ValidationException` es ambiguo** si están `using FluentValidation;` y `using My.Exceptions;` a la vez: hay
  que escribir `My.Exceptions.ValidationException` (como en `UpdatePersonEndpoint` y en
  `DefaultValidatorExtensions`).
- **FluentValidation no puede validar que el objeto raíz sea `null`:** si se le pasa `null` a `Validate`, lanza
  una excepción en vez de devolver un error de validación.
- **CA1812** marca los validators `internal` porque se instancian por reflection → sección `[*Validator.cs]` en
  el `.editorconfig`.
- **El repo de referencia no clona en Windows** ("Filename too long"): leer los archivos con
  `git ls-tree -r --name-only HEAD` + `git show HEAD:<path>` sobre el clone fallido.
- **El IDE mostró errores viejos** (`IValidator<>` no encontrado, orden de usings) en medio de las ediciones; el
  build pasaba. Es caché del diagnóstico.
- El nombre de la extensión tiene un typo heredado: `ValidateAndThrowExeption` (sin la segunda "c"). No se
  renombró.
