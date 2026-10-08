using BuildingBlocks.Domain.Events;
using BuildingBlocks.Mediator.Abstractions;
using BuildingBlocks.Messaging;

namespace BuildingBlocks.Application.DomainEvents;

/*
¿Por qué hay dos clases DomainMessage?

Son dos tipos distintos que comparten nombre. C# lo permite porque uno es genérico y el otro no
(DomainMessage ≠ DomainMessage<T>). Es el mismo patrón que Tuple / Tuple<T> o Nullable / Nullable<T>,
y las dos están en el mismo archivo (por eso el #pragma warning disable SA1402).

- DomainMessage<TEvent> es el mensaje en sí, con Metadata y DomainEvent.
- DomainMessage (static) es solo una factory, con Create(metadata, domainEvent).

La factory hace falta porque person.PullDomainEvents() devuelve los eventos tipados como IDomainEvent.
Pero los handlers están registrados para el tipo concreto (INotificationHandler<DomainMessage<PersonCreatedEvent>>).
Si hicieras new DomainMessage<IDomainEvent>(...), ningún handler lo encontraría. Como el tipo real
se conoce recién en runtime y C# no permite escribir new DomainMessage<domainEvent.GetType()>, la
factory arma el tipo con MakeGenericType y lo instancia con Activator.CreateInstance. DomainEventPublisher
la usa para cada evento.


Analogía
========

Analogía: La carta
En el código: PersonCreatedEvent: el contenido, lo que
pasó en el negocio.
────────────────────────────────────────
Analogía: El sobre
En el código: DomainMessage<TEvent>: envuelve la carta
────────────────────────────────────────
Analogía: Lo escrito en el sobre
En el código: MessageMetadata: número de seguimiento
(MessageId), número de trámite (CorrelationId), "en
respuesta a la carta X" (CausationId) y de qué
expediente se trata (ContentId, el id del agregado)
────────────────────────────────────────
Analogía: La estampilla
En el código: INotification: lo que hace que el correo la acepte
────────────────────────────────────────
Analogía: El correo
En el código: IPublisher / Mediator
────────────────────────────────────────
Analogía: Los destinatarios
En el código: Los DomainEventHandler<TEvent>

Algunos matices que la hacen más precisa:
1. La carta no lleva estampilla. Es justo la decisión de diseño: IDomainEvent no implementa
   INotification. El que escribe la carta (el dominio) no sabe nada del correo. El franqueo lo pone
   quien la manda, que es la capa de Application.

2. Es más un sobre prefranqueado que una estampilla pegada. INotification no se agrega al momento de
   enviar: es parte del tipo, porque DomainMessage<TEvent> ya "viene impreso" con franqueo. Además es
   una interfaz marcadora vacía, igual que una estampilla, que no dice nada del contenido. Solo habilita
   el envío.

3. Es una circular, no una carta con un destinatario. El correo la entrega a todos los suscriptos a
   ese tipo de carta, que pueden ser 0, 1 o N handlers. Si no hay ninguno, no se devuelve al remitente:
   simplemente nadie la lee. Esa es la diferencia con un IRequest, que es una carta certificada con un
   único destinatario obligatorio y respuesta.
*/

/// <summary>
/// Creates <see cref="DomainMessage{TEvent}"/> instances when the domain event type is known only at runtime.
/// </summary>
public static class DomainMessage
{
    #region Public methods

    /// <summary>
    /// Creates a <see cref="DomainMessage{TEvent}"/> whose <c>TEvent</c> is the runtime type of
    /// <paramref name="domainEvent"/>.
    /// </summary>
    /// <remarks>
    /// The events pulled from an aggregate are typed as <see cref="IDomainEvent"/>, but the handlers are
    /// registered for the concrete message (e.g. <c>DomainMessage&lt;PersonCreated&gt;</c>), not for
    /// <c>DomainMessage&lt;IDomainEvent&gt;</c>. C# cannot write <c>new DomainMessage&lt;domainEvent.GetType()&gt;</c>,
    /// so the generic type is built with <c>MakeGenericType</c>.
    /// </remarks>
    /// <param name="metadata">The metadata of the message.</param>
    /// <param name="domainEvent">The domain event to wrap.</param>
    /// <returns>The <see cref="DomainMessage{TEvent}"/>, typed as <see cref="INotification"/> to be published.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="metadata"/> or <paramref name="domainEvent"/> is <see langword="null"/>.</exception>
    public static INotification Create(MessageMetadata metadata, IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(domainEvent);

        Type messageType = typeof(DomainMessage<>).MakeGenericType(domainEvent.GetType());

        // DomainMessage<TEvent> siempre implementa INotification y el constructor publico existe, asi que
        // el cast no falla.
        INotification message = (INotification)Activator.CreateInstance(messageType, metadata, domainEvent)!;
        return message;
    }

    #endregion
}

/// <summary>
/// Wraps a domain event together with its <see cref="MessageMetadata"/> so it can be published with
/// <see cref="IPublisher"/>. The domain event itself knows nothing about the mediator: this wrapper is
/// the <see cref="INotification"/>.
/// </summary>
/// <typeparam name="TEvent">The type of the wrapped domain event.</typeparam>
#pragma warning disable SA1402
public class DomainMessage<TEvent> : INotification
    where TEvent : IDomainEvent
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainMessage{TEvent}"/> class.
    /// </summary>
    /// <param name="metadata"><inheritdoc cref="Metadata" path="/summary"/></param>
    /// <param name="domainEvent"><inheritdoc cref="DomainEvent" path="/summary"/></param>
    /// <exception cref="ArgumentNullException"><paramref name="metadata"/> or <paramref name="domainEvent"/> is <see langword="null"/>.</exception>
    public DomainMessage(MessageMetadata metadata, TEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(domainEvent);

        Metadata = metadata;
        DomainEvent = domainEvent;
    }

    #endregion

    #region Properties

    /// <summary>The metadata of the message.</summary>
    public MessageMetadata Metadata { get; }

    /// <summary>The wrapped domain event.</summary>
    public TEvent DomainEvent { get; }

    #endregion
}
#pragma warning restore SA1402
