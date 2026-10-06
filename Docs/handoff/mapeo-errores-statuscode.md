# Handoff: mapeo-errores-statuscode

## Objetivo

Reemplazar el `string Group` de `Error` por el enum `ErrorType` y que `GlobalExceptionHandler` resuelva el
`HttpStatusCode` **solo** a partir de ese tipo. Los `.Contains` sobre el `ErrorCode` (`.VAL.`, `.DOM.`, etc.)
desaparecen. El `ErrorCode` queda únicamente como código contextual para la UI (para mostrar un mensaje
acorde), no para decidir el status.

## Qué se hizo

Todo commiteado y pusheado en `feature/mediator` (`2da5373`,
`refactor!: reemplaza el group de Error por el enum ErrorType y resuelve el status http solo por tipo`).

| Archivo / área | Cambio | Por qué |
|---|---|---|
| `Core/My.Exceptions/ErrorType.cs` (nuevo, movido desde `BuildingBlocks.Domain/Models`) | Enum `Validation=1, Unauthorized=2, Forbidden=3, NotFound=4, Conflict=5, Failure=6` | Está en Core para que lo vean las excepciones de `My.Exceptions` y `BuildingBlocks.Infra.Http`, que no referencian `BuildingBlocks.Domain`. Los valores siguen el orden de los status (400/401/403/404/409/500); no hay valor 0 (CA1008 en `none`) |
| `BuildingBlocks.Domain/Models/Error.cs` | `Group` (string) → `Type` (`ErrorType`). Ctor: `Error(string code, string message, ErrorType type)` **sin defaults** | Cada error de dominio tiene que declarar su tipo. Efecto colateral: `message` dejó de ser opcional |
| `Records.Persons.Domain/.../DomainErrors.cs` | `ErrorType.NotFound` y `ErrorType.Forbidden` en vez de strings | — |
| `Core/My.Exceptions/ExDataKey.cs` | `ErrorGroup` → `ErrorType` (valor `"ErrorType"`) | — |
| `ValidationException`, `ForbiddenException`, `NotFoundException` (Core) | `SetDefaultErrorCode` → `SetDefaultErrorData`: siguen cargando el `ErrorCode` por defecto (`ERR.VALIDATION` / `ERR.FORBIDDEN` / `ERR.NOTFOUND`) **y además** su `ErrorType` | El código se mantiene como default para la UI (se puede pisar con `AddErrorCode`); el handler ya no lo mira |
| `DomainException` | `Data[ExDataKey.ErrorType] = error.Type` (el enum tipado, no string) | — |
| `DomainValidationException` | Se eliminó `SetDefaultErrorType`; queda un `<remarks>` aclarando que code y type los carga la base | `ValidationException` ya carga `ErrorType.Validation` → era redundante |
| `GlobalExceptionHandler` | Lee `ex.Data[ExDataKey.ErrorType] as ErrorType?`; `ResolveHttpStatusCode(ErrorType?)` es un `switch` (Validation 400, Unauthorized 401, Forbidden 403, NotFound 404, Conflict 409, Failure/sin tipo 500). Se quitó el `SuppressMessage` IDE0051 (el método se usa) | Tipado: sin strings mágicos ni convenciones por substring |
| `.editorconfig` | Comentario en el override `CA1008` (lo había agregado el usuario sin comentario) | Convención del repo: todo override lleva `# CODE: descripción` |

### Decisiones intermedias (descartadas)

1. Primero se guardaba el tipo como **string** en `Data` y el handler comparaba `"Validation"`, `"NotFound"`, …
   porque `Infra.Http` no veía el enum. Se descartó al mover el enum a Core.
2. Se evaluó resolver por `errorCode == ExErrorCodeCore.X || errorType == ...` (sin `.Contains`). Se descartó a
   favor de "solo ErrorType" porque el `ErrorCode` no tiene que decidir el status.

## Verificación hecha

- `dotnet build` de la solución: **0 warnings, 0 errores**.
- **No se probaron los endpoints en ejecución.**

## Cambios de comportamiento

- `DomainErrors.Person.NotFound` y `NotFoundException`: **500 → 404**.
- 401 solo sale con `ErrorType.Unauthorized` (antes también con `.AUTH.` en el código o grupo `"Auth"`; nadie
  lo usaba).
- Una excepción con `.DOM.`/`.VAL.`/`.FORB.`/`.BNS.` en el código y **sin** `ErrorType` ahora da 500. Hoy no
  queda ninguna: `ThrowException2Endpoint` pisa el código con `ERR.DOM.TESTEXCEPTION`, pero al ser
  `ForbiddenException` ya trae `ErrorType.Forbidden` → sigue en 403.
- Los strings viejos de grupo (`"DomainValidation"`, `"Auth"`, `"Domain"`, `"Business"`) ya no existen.

## Pendiente

- Probar en ejecución los status: `/tests/exception1`, `/tests/exception2` (403), validación de FluentValidation
  (400), una persona inexistente (404).
- No hay ningún error/excepción que use `ErrorType.Unauthorized` ni `ErrorType.Conflict` todavía.
- `ResolveHttpStatusCode(Exception)` (resolución por tipo de excepción, "Sin DDD") sigue en el handler sin uso,
  con su `SuppressMessage`; no se tocó.

## Próximos pasos

1. Levantar la API y verificar los 4 casos de arriba (status + `errorCode` en el `ErrorResponse`).
2. Si se necesita 401/409 desde fuera del dominio, decidir si se agregan `UnauthorizedException` /
   `ConflictException` en `My.Exceptions` siguiendo el patrón de `SetDefaultErrorData` (code + type).

## Gotchas

- **Visual Studio quedó con diagnósticos viejos** después de mover `ErrorType` y renombrar `ExDataKey.ErrorGroup`
  (`'ExDataKey' does not contain a definition for 'ErrorType'`, cref sin resolver en
  `DomainValidationException`) mientras `dotnet build` compilaba limpio. Es caché del IDE/ReSharper.
- Dentro de `My.Exceptions` conviven la constante `ExDataKey.ErrorType` y el enum `ErrorType`; no colisionan
  (`ExDataKey.ErrorType` siempre va calificado), pero ojo al leer.
- `nameof(ErrorType.X)` vs `.ToString()`: con el enum tipado en `Data` ya no hace falta ninguno de los dos.
