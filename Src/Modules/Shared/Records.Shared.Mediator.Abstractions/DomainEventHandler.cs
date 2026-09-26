namespace Records.Shared.Mediator.Abstractions;

/// <summary>
/// Base class for domain event handlers. Any number of handlers (including none) can exist per
/// <typeparamref name="TEvent"/>; all of them run when the event is published.
/// </summary>
/// <typeparam name="TEvent">Domain event type handled.</typeparam>
public abstract class DomainEventHandler<TEvent> : NotificationHandler<TEvent>
    where TEvent : IDomainEvent
{
}
