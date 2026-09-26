namespace Records.Shared.DomainEvents.Abstractions;

/// <summary>
/// Handles a single domain event type.
/// Implementations are resolved by <see cref="IDomainEventDispatcher"/> via <c>GetServices</c>,
/// so any number of handlers (including none) can be registered per <typeparamref name="TEvent"/>.
/// </summary>
/// <remarks>
/// Inherit from <see cref="DomainEventHandler{TEvent}"/> instead of implementing this interface
/// directly, so the non-generic <see cref="IDomainEventHandler"/> is implemented for you.
/// Although <typeparamref name="TEvent"/> is contravariant, the default Microsoft DI container does
/// not resolve variant registrations: a handler for a base event type (e.g.
/// IDomainEventHandler&lt;IDomainEvent&gt;) is not invoked for derived events.
/// </remarks>
/// <typeparam name="TEvent">Domain event type handled.</typeparam>
public interface IDomainEventHandler<in TEvent> : IDomainEventHandler
    where TEvent : IDomainEvent
{
    /// <summary>
    /// Asynchronously handles the given domain event.
    /// </summary>
    /// <param name="event">Event object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Handle(TEvent @event, CancellationToken cancellationToken = default);
}

/// <summary>
/// Non-generic contract used by <see cref="IDomainEventDispatcher"/> to invoke a handler when it only
/// knows the event as <see cref="IDomainEvent"/> (its concrete type is known only at runtime).
/// </summary>
/// <remarks>
/// Do not implement this interface directly: inherit from <see cref="DomainEventHandler{TEvent}"/>,
/// which implements it by casting the event to <c>TEvent</c> and forwarding to the typed
/// <see cref="IDomainEventHandler{TEvent}.Handle(TEvent, CancellationToken)"/>.
/// </remarks>
public interface IDomainEventHandler
{
    /// <summary>
    /// Asynchronously handles the given domain event.
    /// </summary>
    /// <param name="event">Event object. Its runtime type must be the handler's event type.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Handle(IDomainEvent @event, CancellationToken cancellationToken = default);
}
