namespace Records.Shared.DomainEvents.Abstractions;

/// <summary>
/// Base class for domain event handlers. Concrete handlers only implement the typed
/// <see cref="Handle(TEvent, CancellationToken)"/>; the non-generic <see cref="IDomainEventHandler"/>
/// needed by <see cref="IDomainEventDispatcher"/> is implemented here, once, by casting the event.
/// </summary>
/// <typeparam name="TEvent">Domain event type handled.</typeparam>
public abstract class DomainEventHandler<TEvent> : IDomainEventHandler<TEvent>
    where TEvent : IDomainEvent
{
    /// <inheritdoc/>
    public abstract Task Handle(TEvent @event, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    Task IDomainEventHandler.Handle(IDomainEvent @event, CancellationToken cancellationToken)
    {
        Task handleTask = Handle((TEvent)@event, cancellationToken);
        return handleTask;
    }
}
