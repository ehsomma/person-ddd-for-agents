namespace Records.Shared.DomainEvents.Abstractions;

/// <summary>
/// Publishes a domain event to every <see cref="IDomainEventHandler{TEvent}"/> registered.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Asynchronously dispatches an event to all of its handlers.
    /// </summary>
    /// <remarks>
    /// Handlers are resolved by the event's runtime type (<c>@event.GetType()</c>), not by the
    /// static type of the argument, so events can be dispatched from an
    /// <c>IEnumerable&lt;IDomainEvent&gt;</c> without casting. Handlers run sequentially, in
    /// registration order; if none is registered the call completes without doing anything. The
    /// first handler that throws stops the remaining ones and its exception is propagated.
    /// </remarks>
    /// <param name="event">Event object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the dispatch operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
    Task Dispatch(IDomainEvent @event, CancellationToken cancellationToken = default);
}
