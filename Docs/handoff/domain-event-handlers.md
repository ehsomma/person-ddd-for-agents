# Handoff: domain-event-handlers

## Objetivo

Implementar en la capa de Application los handlers de los eventos de dominio de `Person`. Por ahora solo
loguean, para comprobar que los eventos se publican y llegan a sus handlers. De paso, alinear
`DomainMessage<TEvent>` con el repo de referencia
([`ehsomma/ddd-cqrs-microservices`](https://github.com/ehsomma/ddd-cqrs-microservices)) haciéndolo heredar
de `Message<TContent>`, para dejar la base lista para Outbox e Integration Events.

## Qué se hizo

| Archivo | Cambio | Por qué |
|---|---|---|
| `Records.Persons.Application/Person/Events/PersonCreated/PersonCreatedEventHandler.cs` | Handler modelo: `internal sealed`, hereda de `DomainEventHandler<PersonCreatedEvent>`, inyecta `ILogger<T>` y loguea `PersonId`, `MessageId` y `CorrelationId` con message template. Devuelve `Task.CompletedTask`. | Ver las decisiones de abajo. Commiteado antes en `ac607b4`; acá cambia `.DomainEvent` → `.Content` y se saca un punto de más en el summary. |
| `.../Events/PersonUpdated/`, `PersonDeleted/`, `PersonalAssetAdded/` (nuevos) | Un handler por evento, copia exacta del modelo. `PersonalAssetAdded` loguea la `Description` del activo. | Son los mismos que tiene el repo de referencia. Del activo se loguea la descripción y no el `Id`: el modelo de dominio no recibe los ids que genera el insert, así que daría 0. |
| `Records.Persons.Application.csproj` | `Microsoft.Extensions.Logging.Abstractions` 10.0.0 (commiteado en `ac607b4`). | Para `ILogger<T>` sin depender de Serilog. |
| `BuildingBlocks.Messaging/Message.cs` (nuevo) | `Message<TContent>` con `Metadata` + `Content`, `where TContent : notnull`, propiedades `{ get; }`. Sin `INotification`. | Base común de mensajes, igual que en la referencia. No conoce el transporte: el Outbox o el broker van a usar `Message<T>` directo. |
| `BuildingBlocks.Application/DomainEvents/DomainMessage.cs` | `DomainMessage<TEvent> : Message<TEvent>, INotification`. Solo tiene el ctor, que llama a la base; se borró la propiedad `DomainEvent`. La factory `DomainMessage.Create` no cambia. Se actualizaron las notas/analogía del usuario (comentario al principio del archivo). | Pedido del usuario. Decisión: renombrar a `Content` como en la referencia, sin alias `DomainEvent`. |
| `BuildingBlocks.Messaging/TODO-IntegrationEvents.md` | Nueva sección "Qué ya está"; pendientes renumerados. | `Message<T>` y la herencia ya están hechos. |
| `.editorconfig` | `CA1873` en `silent` (lo agregó el usuario) + comentario. | Regla del usuario: toda override lleva comentario. |

Decisiones de diseño, ya discutidas con el usuario:

- **`ILogger<T>` inyectado, no `Serilog.Log` estático como en la referencia:**
  - Application no depende de Serilog.
  - La dependencia queda visible en el constructor.
  - `SourceContext` sale automático con el nombre de la clase.
  - Se puede testear.
- **`Handle` sin `async`:** la firma ya devuelve `Task`, y `async` es un detalle de implementación. Ponerlo sin ningún `await` da CS1998. Se agrega cuando haya algo que esperar (p.ej. guardar en el Outbox).
- **Clases base (`DomainEventHandler<TEvent>`, `CommandHandler`) en vez de las interfaces `IDomainEventHandler`/`ICommandHandler` de la referencia:**
  - El mediator es propio. Necesita el `Handle(INotification)` no genérico de `INotificationHandlerBase`, y la base lo implementa una sola vez.
  - `DomainEventHandler<TEvent>` arma el sobre (`DomainMessage<TEvent>`) solo y limita `TEvent` a `IDomainEvent`.
- **El handler de `PersonalAssetCreatedEvent` no se hizo:** el dominio no lo dispara (está comentado en `PersonalAsset.cs:46`) y la referencia tampoco tiene ese handler.

## Verificación hecha

- `dotnet build Src/Apps/Records.Persons.Api.V1`: sin errores ni warnings.
- Sin base de datos: una app de consola descartable en el scratchpad registra `AddMediator` +
  `AddApplication` + logging, sin persistencia. Con un `Person` hace Create, `AddPersonalAsset`, `Update` y
  `Delete`, y publica `PullDomainEvents()` con `IDomainEventPublisher`. Los 4 handlers loguean en orden, con
  el `PersonId` correcto y `CorrelationId == MessageId` (mensajes raíz).
- **No se probó end-to-end con la API:** no hay base de datos todavía.

## Pendiente

- Probar con la API y la DB que los eventos se publican **después** del commit (Create, Update, Delete).
- Integration Events + Outbox: ver `Src/BuildingBlocks/BuildingBlocks.Messaging/TODO-IntegrationEvents.md`
  (`Message.Build<T>`, `Build(OutboxMessage)`, `init` para MassTransit, lo que le falta a `MessageMetadata` y
  guardar en el Outbox desde estos handlers).
- Con `CA1873` en `silent`, los `if (_logger.IsEnabled(...))` de los handlers ya no son obligatorios.
  Decidir si se mantienen (evitan boxear los `Guid`) o se sacan.

## Próximos pasos concretos

1. Cuando haya DB: levantar la API, hacer POST, PUT y DELETE de persona y buscar `... handled` en la salida
   de Serilog.
2. Reemplazar el log de cada handler por la lógica real: crear el integration event, encadenar la
   metadata con `new MessageMetadata(notification.Metadata)` y guardar `Message<TIntegrationEvent>` en el
   Outbox. Ahí el `Handle` pasa a `async`.

## Gotchas

- Si una app de prueba usa `ValidateOnBuild = true`, DI falla: valida también los command handlers, que
  necesitan `IUnitOfWork` (persistencia). Para probar solo eventos, desactivarlo.
- `Person.Create` exige `Address` (`DomainValidationException` si es `null`).
- `person.Update(person)` (pasar la misma instancia como `updatedData`) tira "Collection was modified",
  porque recorre `PersonalAssets` mientras le agrega. Hay que pasar otra instancia.
- `CA1873` saltó por pasar `Guid` (boxing) a `LogInformation`. Se resolvió con el guard `IsEnabled` y
  después el usuario la puso en `silent`.
