using Records.Shared.Domain.Events;
using Records.Shared.Mediator.Abstractions;
using Records.Shared.Messaging;

namespace Records.Shared.Application.DomainEvents;

/// <summary>
/// Creates <see cref="DomainMessage{TEvent}"/> instances when the domain event type is known only at runtime.
/// </summary>
public static class DomainMessage
{
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

    /// <summary>The metadata of the message.</summary>
    public MessageMetadata Metadata { get; }

    /// <summary>The wrapped domain event.</summary>
    public TEvent DomainEvent { get; }
}
#pragma warning restore SA1402
