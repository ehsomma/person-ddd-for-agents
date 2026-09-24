namespace Records.Shared.DomainEvents.Abstractions;

public interface IDomainEventHandler
{
    Task Handle(IDomainEvent @event, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a domain event handler for a TEvent.
/// </summary>
/// <typeparam name="TEvent">The event type with a domain event.</typeparam>
public interface IDomainEventHandler<in TEvent> : IDomainEventHandler
    where TEvent : IDomainEvent
{
    Task Handle(TEvent @event, CancellationToken cancellationToken = default);
}
