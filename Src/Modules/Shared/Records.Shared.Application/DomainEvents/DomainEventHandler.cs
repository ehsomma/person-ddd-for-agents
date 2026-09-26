using Records.Shared.Domain.Events;
using Records.Shared.Mediator.Abstractions;

namespace Records.Shared.Application.DomainEvents;

/// <summary>
/// Base class for domain event handlers. Any number of handlers (including none) can exist per
/// <typeparamref name="TEvent"/>; all of them run when the event is published.
/// </summary>
/// <remarks>
/// The handler receives the whole <see cref="DomainMessage{TEvent}"/> (not just the event) so it also
/// has the message metadata, e.g. to create the metadata of the messages it causes.
/// </remarks>
/// <typeparam name="TEvent">Domain event type handled.</typeparam>
public abstract class DomainEventHandler<TEvent> : NotificationHandler<DomainMessage<TEvent>>
    where TEvent : IDomainEvent
{
}
