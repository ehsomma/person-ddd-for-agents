# Handoff: api-xml-comments

## Objetivo

Documentar los endpoints de la API con comentarios XML para que la información se vea bien en la UI de Scalar
(`/v1/docs`): el título de cada operación en el menú, su descripción, la descripción y el ejemplo de cada
parámetro (headers, ruta, query y body) y las respuestas posibles con su schema. En el camino se definieron las
convenciones de los endpoints y se agregaron al skill `team-standards`.

## Qué se hizo

Todo en `feature/mediator`.

### Archivos

| Archivo | Cambio | Por qué |
|---|---|---|
| `BuildingBlocks.Infra.OpenApi.DependencyInjection/XmlParamCommentsOperationTransformer.cs` (nuevo) | Operation transformer que vuelve a aplicar los `<param>` del método del endpoint, leyéndolos del `.xml` de documentación del assembly: la descripción y el ejemplo del body salen solo del parámetro del body, y los headers/query/route con un nombre de binding distinto al de C# (p.ej. `correlationId` → `X-Correlation-Id`) reciben su descripción y su `example`. El comentario de la clase explica el bug. | Corrige un bug del source generator de Microsoft (ver gotchas). Se escribe una vez y sirve para todas las APIs, sin tocar los endpoints. |
| `BuildingBlocks.Infra.OpenApi.DependencyInjection/ServiceCollectionExtensions.cs` | `ConfigureOpenApiCustom` registra el transformer. | Corre después del transformer del generator, porque este se registra antes de ejecutar las options de `AddOpenApi`. |
| `BuildingBlocks.Infra.Http.DependencyInjection/ServiceCollectionExtensions.cs` | `MapEndpoints` mapea todos los endpoints dentro de un grupo raíz sin prefijo y llama a `AddCommonErrorResponses(rootGroup)`, que agrega el **500** (`ErrorResponse`) a todos los endpoints y el **400** (`ErrorResponse`) solo a los que reciben body, salvo que el endpoint ya declare su propio 400. Se agregaron las regions de la clase. | Son errores que devuelve el `GlobalExceptionHandler`, iguales para toda la API: así no se declaran en cada endpoint (equivalente a un `IOperationFilter` de Swashbuckle). |
| Los 10 `*Endpoint.cs` de `Records.Persons.Api.V1` (Person y Test) | Método del endpoint `public` (con un `NOTE`); `<summary>` corto en infinitivo; `<remarks>` solo con lo que hace; `<returns>`; orden de parámetros y atributo explícito en todos; `.Produces<ErrorResponse>(404)` en GetById, Update y Delete; `.Produces<ErrorResponse>(400)` en `by-gender-and-birthdate` (no tiene body); `.Produces<ErrorResponse>(403)` en `tests/exception2`. Regions en los endpoints de test. | Ver decisiones. |
| `Records.Persons.Api.V1/Program.cs` | (Lo hizo el usuario) Nueva descripción y título del documento, Scalar en `/v1/docs`. | — |
| `Records.Persons.Api.V1/Properties/launchSettings.json` | (Lo hizo el usuario) `launchUrl: v1/docs`. | — |
| `.claude/skills/team-standards/SKILL.md` | Nueva sección **"Endpoints de minimal APIs"** con las 5 reglas (orden, atributos, `public`, summary/remarks, respuestas). | No hay analyzer que lo marque: sin el skill, un endpoint nuevo puede copiar un patrón distinto. |

### Decisiones (discutidas con el usuario)

* **El método del endpoint es `public`**: el generator solo lee los comentarios XML de métodos públicos (la clase
  puede seguir `internal`).
* **Transformer en vez de `[AsParameters]`**: la otra forma de esquivar el bug era agrupar los parámetros en una
  clase por endpoint con `[AsParameters]`. El usuario no quiso una clase "falsa" por endpoint.
* **`<summary>` = título corto en infinitivo** (`Crear persona.`, `Obtener persona por ID.`): es lo que Scalar muestra
  en el menú y distingue endpoints con la misma ruta. Lleva punto porque lo pide StyleCop (SA1629).
* **`<remarks>` solo con lo que hace el endpoint**: sin validaciones, códigos de error ni casos borde, que ya están
  en las `<response>`. Si cambian, es fácil olvidarse de actualizar el texto.
* **Orden de parámetros**: header → ruta/query → body → servicios → `CancellationToken`. **Atributo explícito en
  todos** (`[FromHeader]`, `[FromRoute]`, `[FromQuery]`, `[FromBody]`, `[FromServices]`). Solo `CancellationToken` y
  `HttpContext` quedan sin atributo (no existe), al final.
* **El 400 común depende de si el endpoint recibe body** (no del verbo): se detecta con el `IAcceptsMetadata` que
  minimal APIs agrega cuando hay un `[FromBody]`.
* Se sacó el `.Accepts<Dto.Person>(...)` de Create (redundante con `[FromBody]`). Quedó el `.Produces<Dto.Person>(200)`
  de Create, que también es redundante (sale del tipo de retorno) pero el usuario lo dejó.

## Pendiente

* Probar todo en Scalar con la API real. Se verificó el `/openapi/v1.json` levantando una copia en otro puerto (5199),
  pero esa copia no tenía conexión a SQL Server, así que no se llamó a los endpoints de Person contra la base.
* Un endpoint sin body también puede devolver 400 (p.ej. si falta un header o un query obligatorio, porque
  `ThrowOnBadRequest` está activo) y no lo documenta. Se habló de cambiar el criterio a "tiene algún parámetro del request"; quedó así.
* Las respuestas salen en Scalar como 200, 500, 400 (las comunes se agregan al final). Se puede ordenar por código
  en el transformer de OpenAPI si molesta.
* `Program.cs` tiene un `using System.Runtime.InteropServices;` que no parece usarse (lo agregó el usuario).

## Próximos pasos

1. Reiniciar la API y revisar en Scalar el menú, cada operación y el ejemplo del body en "Test Request".
2. Decidir si el 400 se documenta también en los endpoints sin body que tienen header/ruta/query.
3. En cada endpoint nuevo, seguir la sección "Endpoints de minimal APIs" del skill `team-standards`.

## Gotchas

* **Bug del source generator** (`Microsoft.AspNetCore.OpenApi` 10.0.x, `XmlCommentOperationTransformer`): cada
  `<param>` cuyo nombre no coincide con un parámetro de la operación se escribe en la descripción del body (gana el
  último) y su `example` pisa el ejemplo del body. Pasa con los servicios, el `CancellationToken` y los headers con
  `Name` distinto. Síntomas: el body mostraba "Token de cancelación del request." y Scalar decía "Invalid JSON
  body" (el ejemplo de un header, que no era JSON, terminaba como ejemplo del body). Lo corrige `XmlParamCommentsOperationTransformer`.
* **Métodos privados**: el generator no los incluye en su cache de comentarios, sin error ni warning. Para ver qué
  tomó: compilar con `-p:EmitCompilerGeneratedFiles=true` y buscar el `M:` del método en
  `OpenApiXmlCommentSupport.generated.cs`.
* **Una sola respuesta**: si la operación tiene una sola respuesta, el generator usa el `<returns>` como su
  descripción. Pasaba en `tests/exception1` (el 500 decía "Nunca devuelve nada..."); se resolvió con un
  `<response code="500">` explícito.
* **`<response>` sin `.Produces`**: si el código no existe en la operación, la `<response>` no se muestra (pasaba con
  los 404).
* **Conventions del grupo y metadata inferida**: las conventions comunes del grupo corren antes de que minimal APIs
  infiera la metadata de los parámetros (`IAcceptsMetadata`), por eso el 400 se agrega en `Finally`. `Finally` es un
  default interface method de `IEndpointConventionBuilder`: hay que llamarlo a través de la interfaz (sobre
  `RouteGroupBuilder` no compila).
* **Scalar agrupa los parámetros por ubicación** (path, query, headers, body) con un orden fijo, sin importar el orden
  del método ni del documento OpenAPI.
* **`tests/exception2` devuelve 403**, no 500: `ForbiddenException` se resuelve a 403 en el `GlobalExceptionHandler`.
* Para probar mientras la API está corriendo (los binarios quedan bloqueados): `dotnet build -o <otra carpeta>` y
  `dotnet <dll> --urls http://localhost:5199`. Hot Reload no aplica cambios de visibilidad de métodos.
