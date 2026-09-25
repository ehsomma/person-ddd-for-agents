# Handoff: domain-events-sin-mediatr

## Objetivo

Armar un mecanismo de **eventos de dominio sin MediatR**, análogo al CQRS casero de
`Src/Modules/Shared/Records.Shared.Cqrs*`: despachar un evento a **todos** sus handlers (0..N), como
`IMediator.Publish`. Se partió de una implementación .NET 6 de otro proyecto, adaptada a .NET 10.

## Qué se hizo

1. Estructura de proyectos (commit `39b26eb`): `Records.Shared.DomainEvents.Abstractions`,
   `Records.Shared.DomainEvents`, `Records.Shared.DomainEvents.DependencyInjection` (net10.0, carpeta de
   solución `Shared`, referencias espejo de CQRS).
2. El usuario volcó el código .NET 6; se revisó y refactorizó (**sin commitear todavía**):

| Archivo | Cambio | Por qué |
|---|---|---|
| `Abstractions/IDomainEventHandler.cs` | Se mantiene el diseño .NET 6: `IDomainEventHandler` no genérico + `IDomainEventHandler<in TEvent> : IDomainEventHandler`; docs nuevas | El dispatcher solo conoce `IDomainEvent`; el no genérico le permite llamar `Handle` sin conocer `TEvent`. La herencia obliga (en compilación) a implementar ambos `Handle`, así ningún handler queda salteado en silencio |
| `Abstractions/DomainEventHandler.cs` (nuevo) | Clase base abstracta: los handlers implementan solo `Handle(TEvent)`; el `Handle(IDomainEvent)` se implementa explícitamente acá con el cast | Evita el boilerplate del cast en cada handler (opción "A" elegida por el usuario) |
| `Abstractions/IEventDispatcher.cs` → `IDomainEventDispatcher.cs` | Renombrado (decisión del usuario) + doc corregida | Alinear con `DomainEventDispatcher`/`ICommandDispatcher`; la doc decía "single handler" |
| `Abstractions/IDomainEvent.cs` | Se mantiene `AggregateId` (string) y `EventOccurredAtUtc`; se quitó la mención a `MessageMetadata/ContentId` y los `#region` | Concepto del proyecto viejo que acá no existe |
| `DomainEvents/DomainEventDispatcher.cs` | Código del usuario con el cast corregido: `GetServices(handlerType).Cast<IDomainEventHandler>()` + docs | Su versión casteaba a `IDomainEventHandler<IDomainEvent>`: con `in TEvent` esa conversión es imposible (un handler de `PersonCreated` no es handler de *cualquier* evento) → el `as` daba siempre `null` y **ningún handler se ejecutaba, sin error** |
| `DomainEvents.DependencyInjection/ServiceCollectionExtensions.cs` | Registra `IDomainEventDispatcher` (Scoped) + Scan; nuevo `EnsureNoDuplicateHandlerRegistrations` | Si se escanea dos veces el mismo assembly cada handler correría dos veces; falla rápido (espejo de `EnsureSingleHandlerPerContract` de CQRS) |
| `.editorconfig` | `CA1711` global → sección `[*EventHandler.cs]` comentada (+ `CA1812` ahí) | Regla del usuario: toda override con comentario; CA1812 es falso positivo para handlers instanciados por reflexión |

Semántica elegida (decisión del usuario): handlers **secuenciales**, en orden de registro; sin handlers
no hace nada; la primera excepción corta y se propaga (como `ForeachAwaitPublisher` de MediatR). Motivo:
los handlers comparten `IDbConnection`/UoW scoped de Dapper, que no son thread-safe.

## Verificación hecha

- `dotnet build Records.sln`: 0 errores; solo quedan 3 `CS0618` preexistentes en
  `Records.Shared.Infra.OpenApi.DependencyInjection` (ajenos a esta tarea). Los `CA1016` de los
  proyectos vacíos desaparecieron al tener código.
- Consola descartable en el scratchpad (no está en el repo): 2 handlers para un evento, 1 para otro,
  ninguno para un tercero → `H1:1 | H2:1 | H3:2 | H1:3 | H2:3` (orden correcto, el evento sin handlers
  no falla); `Dispatch(null)` → `ArgumentNullException`; `AddDomainEvents` dos veces →
  `InvalidOperationException` con detalle.

## Pendiente

- Commitear (usar skill `git-commit`). En el working tree también hay cambios **del usuario** no
  relacionados: `CommandDispatcher.cs` (doc) y `.editorconfig`
  `csharp_style_pattern_matching_over_as_with_null_check = false` — este último probablemente se puso
  para silenciar el `as` del dispatcher viejo, que ya no existe; evaluar volverlo a `true`.
- `dotnet_diagnostic.CA1040.severity = none` en `.editorconfig` sigue sin comentario `# CODE:`.
- Integración con el dominio (fuera de alcance hasta ahora).

## Próximos pasos concretos

1. Agregar al agregado raíz una colección de eventos (`DomainEvents` / `RaiseDomainEvent` /
   `ClearDomainEvents`), probablemente en el proyecto de dominio/shared kernel.
2. Decidir quién despacha: p. ej. el command handler (o un decorator/UoW) después de persistir, iterando
   `aggregate.DomainEvents` y llamando `IDomainEventDispatcher.Dispatch` (acepta `IDomainEvent`, resuelve
   por tipo runtime).
3. Llamar `services.AddDomainEvents(typeof(AssemblyReference).Assembly)` en
   `Src/Apps/Records.Persons.Api.V1/Program.cs` junto a `AddCqrs` (línea ~41).
4. Crear el primer evento + handler real (archivo `*EventHandler.cs`, `internal sealed`, heredando de
   `DomainEventHandler<TEvent>`) para que caiga
   en la sección del `.editorconfig`.

## Gotchas

- **No usar `dotnet sln add`** sobre `Records.sln`: agrega plataformas x64/x86 a todos los proyectos y
  pasa el archivo a LF. Editar a mano respetando CRLF + BOM.
- La contravarianza `in TEvent` **no** la honra el contenedor de MS DI: un
  `IDomainEventHandler<IDomainEvent>` no recibe eventos concretos (documentado en el XML doc).
- Alternativas descartadas para el dispatcher: reflexión directa (`MethodInfo.Invoke`: reflexión por
  llamada y excepciones envueltas en `TargetInvocationException`) y wrapper genérico cacheado estilo
  MediatR (más eficiente pero más difícil de leer; se implementó y se revirtió a pedido del usuario).
- Los handlers concretos deben heredar de `DomainEventHandler<TEvent>`; Scrutor los registra también
  como `IDomainEventHandler` no genérico (por `AsImplementedInterfaces`), inofensivo.
- El repo tiene `CA2007` apagado y no usa `ConfigureAwait(false)`; no agregarlo.
- Estándar del equipo: `return` solo de una variable → en `DomainEventHandler` se asigna `handleTask`
  antes del `return`.
- La carpeta real del repo es `Docs/` (mayúscula).
