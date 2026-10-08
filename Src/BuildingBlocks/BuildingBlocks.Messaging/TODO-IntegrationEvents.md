# Pendiente: Integration Events + Outbox

Cuando se quiera implementar **Integration Events** (mensajes entre microservicios, publicados con un
broker tipo MassTransit/RabbitMQ usando el patrón **Outbox**), hay que traer lo que falta del repo
viejo [`ehsomma/ddd-cqrs-microservices`](https://github.com/ehsomma/ddd-cqrs-microservices/tree/master).

## Qué ya está

- **`Message<TContent>`** (`BuildingBlocks.Messaging/Message.cs`): base común de mensajes, sin dependencias y
  **sin** `INotification`, con `Metadata` + `Content`. Referencia:
  [`Records.Shared.Messaging/Message.cs`](https://github.com/ehsomma/ddd-cqrs-microservices/blob/master/Src/Services/Shared/Records.Shared.Messaging/Message.cs).
- **`DomainMessage<TEvent> : Message<TEvent>, INotification`** (`BuildingBlocks.Application/DomainEvents`). Los
  `DomainEventHandler<TEvent>` acceden al evento con `message.Content`. Referencia:
  [`Records.Shared.Application/Messaging/DomainMessage.cs`](https://github.com/ehsomma/ddd-cqrs-microservices/blob/master/Src/Services/Shared/Records.Shared.Application/Messaging/DomainMessage.cs).

## Qué hay que hacer

1. **Completar `Message<TContent>`** con lo que en el repo viejo solo usaban los integration events y el Outbox:
   - La factory estática `Message.Build<TContent>(metadata, content)`, que arma el `Message<T>` con
     `MakeGenericType`, igual que `DomainMessage.Create`.
   - Si MassTransit tiene que deserializar `Message<T>`, pasar `Metadata`/`Content` de `{ get; }` a `{ get; init; }`
     (hoy son inmutables).
2. **Outbox**: `OutboxMessage`, `IOutboxRepository` y `Message.Build(OutboxMessage)` (rehidrata desde el
   Outbox: deserializa el contenido y reconstruye el `Message<T>`). En el repo viejo, los
   contratos (`OutboxMessage`, `IOutboxRepository`, `IOutboxMapper`, `IMessageMetadata`) estaban en
   `BuildingBlocks.Messaging`, y la implementación SQL (`OutboxRepository`, `OutboxMapper`) en
   `BuildingBlocks.Infra.Outbox.Sql`.
   - Traer también **`IMessageMetadata`**: `OutboxMessage` la implementa porque la metadata se guarda
     **aplanada** en la tabla. Si se agrega una propiedad a `MessageMetadata`, hay que agregarla en
     `IMessageMetadata`, `OutboxMessage`, `OutboxRepository.SaveAsync()`, `OutboxMapper.Map()` y
     `Message.Build()`.
3. **`MessageMetadata`** (ya existe acá, sin Outbox). Hay que sumarle lo que se sacó:
   - `PublishedOnUtc` (hoy no está porque solo tiene sentido con Outbox).
   - Constructor sin parámetros: MassTransit lo usa al deserializar y después inicializa las propiedades
     (por eso son `get; init;`).
   - Referencia:
     [`Records.Shared.Messaging/MessageMetadata.cs`](https://github.com/ehsomma/ddd-cqrs-microservices/blob/master/Src/Services/Shared/Records.Shared.Messaging/MessageMetadata.cs).
4. **Desde el handler de dominio, guardar en el Outbox**: crear el integration event, encadenar la
   metadata con `new MessageMetadata(message.Metadata)` (misma correlation, causation = el mensaje de
   dominio) y guardar el `Message<TIntegrationEvent>`. Referencia:
   [`PersonCreatedEventHandler.cs`](https://github.com/ehsomma/ddd-cqrs-microservices/blob/master/Src/Services/Persons/Records.Persons.Application/Persons/Events/PersonCreated/PersonCreatedEventHandler.cs).

Repo de referencia: [https://github.com/ehsomma/ddd-cqrs-microservices](https://github.com/ehsomma/ddd-cqrs-microservices)