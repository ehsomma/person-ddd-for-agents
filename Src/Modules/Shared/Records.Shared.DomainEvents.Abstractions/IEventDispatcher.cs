namespace Records.Shared.DomainEvents.Abstractions;

/// <summary>
/// Resolves the single <see cref="IDomainEventHandler{TEvent}"/> registered
/// for an event type and dispatches the event to it.
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Asynchronously dispatches an event to its handler.
    /// </summary>
    /// <param name="event">Event object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the dispatch operation.</returns>
    Task Dispatch(IDomainEvent @event, CancellationToken cancellationToken = default);
}
