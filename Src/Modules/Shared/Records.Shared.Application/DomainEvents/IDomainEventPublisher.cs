using Records.Shared.Domain.Events;

namespace Records.Shared.Application.DomainEvents;

/// <summary>
/// Publishes domain events to their <see cref="DomainEventHandler{TEvent}"/>s, wrapping each one in a
/// <see cref="DomainMessage{TEvent}"/> with its metadata.
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Asynchronously publishes the given domain events, one after the other and in order.
    /// </summary>
    /// <remarks>
    /// Each event gets root metadata whose <c>ContentId</c> is the event's <see cref="IDomainEvent.AggregateId"/>.
    /// The first handler that throws stops the remaining events and handlers and its exception is propagated.
    /// </remarks>
    /// <param name="domainEvents">The domain events to publish (e.g. the ones pulled from an aggregate).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the publish operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="domainEvents"/> is <see langword="null"/>.</exception>
    Task Publish(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
