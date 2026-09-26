# Pendiente: Integration Events + Outbox

Cuando se quiera implementar **Integration Events** (mensajes entre microservicios, publicados con un
broker tipo MassTransit/RabbitMQ usando el patrón **Outbox**), hay que traer lo que falta del repo
viejo [`ehsomma/ddd-cqrs-microservices`](https://github.com/ehsomma/ddd-cqrs-microservices/tree/master).

## Por qué hoy no está

En el repo viejo, `Message<TContent>` cumplía dos funciones, y las dos dependen de Integration Events + Outbox:

1. **Base común de mensajes**: `DomainMessage<T>` heredaba de `Message<T>`, y los integration events se
   guardaban en el Outbox como `Message<PersonCreatedIntegrationEvent>`. Vive en `Shared.Messaging`
   **sin** `INotification`, porque ese mensaje no va al mediator sino al broker.
2. **Rehidratar desde el Outbox**: `Message.Build(OutboxMessage)` deserializa el contenido y reconstruye
   el `Message<T>` para publicarlo.

Mientras solo haya mensajes de dominio, una base con una única subclase no aporta nada: por eso hoy
`DomainMessage<TEvent>` (en `Records.Shared.Application/DomainEvents`) tiene directamente `Metadata` +
`DomainEvent`.

## Qué hay que hacer

1. **`Message<TContent>` en este proyecto** (`Records.Shared.Messaging`), sin dependencias, con `Metadata` +
   `Content`. Referencia:
   [`Records.Shared.Messaging/Message.cs`](https://github.com/ehsomma/ddd-cqrs-microservices/blob/master/Src/Services/Shared/Records.Shared.Messaging/Message.cs).
2. **`DomainMessage<TEvent> : Message<TEvent>, INotification`**. Referencia:
   [`Records.Shared.Application/Messaging/DomainMessage.cs`](https://github.com/ehsomma/ddd-cqrs-microservices/blob/master/Src/Services/Shared/Records.Shared.Application/Messaging/DomainMessage.cs).
   - **Ojo con el nombre de la propiedad**: hoy es `DomainEvent`, la base usaría `Content`. Decidir entre
     renombrarla a `Content` (tocando los `DomainEventHandler<TEvent>` que existan) o mantener `DomainEvent`
     como alias de `Content`.
3. **Outbox**: `OutboxMessage`, `IOutboxRepository` y `Message.Build(OutboxMessage)`. En el repo viejo, los
   contratos (`OutboxMessage`, `IOutboxRepository`, `IOutboxMapper`, `IMessageMetadata`) estaban en
   `Records.Shared.Messaging`, y la implementación SQL (`OutboxRepository`, `OutboxMapper`) en
   `Records.Shared.Infra.Outbox.Sql`.
   - Traer también **`IMessageMetadata`**: `OutboxMessage` la implementa porque la metadata se guarda
     **aplanada** en la tabla. Si se agrega una propiedad a `MessageMetadata`, hay que agregarla en
     `IMessageMetadata`, `OutboxMessage`, `OutboxRepository.SaveAsync()`, `OutboxMapper.Map()` y
     `Message.Build()`.
4. **`MessageMetadata`** (ya existe acá, sin Outbox). Hay que sumarle lo que se sacó:
   - `PublishedOnUtc` (hoy no está porque solo tiene sentido con Outbox).
   - Constructor sin parámetros: MassTransit lo usa al deserializar y después inicializa las propiedades
     (por eso son `get; init;`).
   - Referencia:
     [`Records.Shared.Messaging/MessageMetadata.cs`](https://github.com/ehsomma/ddd-cqrs-microservices/blob/master/Src/Services/Shared/Records.Shared.Messaging/MessageMetadata.cs).
5. **Desde el handler de dominio, guardar en el Outbox**: crear el integration event, encadenar la
   metadata con `new MessageMetadata(message.Metadata)` (misma correlation, causation = el mensaje de
   dominio) y guardar el `Message<TIntegrationEvent>`. Referencia:
   [`PersonCreatedEventHandler.cs`](https://github.com/ehsomma/ddd-cqrs-microservices/blob/master/Src/Services/Persons/Records.Persons.Application/Persons/Events/PersonCreated/PersonCreatedEventHandler.cs).

Repo de referencia: [https://github.com/ehsomma/ddd-cqrs-microservices](https://github.com/ehsomma/ddd-cqrs-microservices)