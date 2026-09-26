namespace Records.Shared.Mediator.Abstractions;

/// <summary>
/// Represents an event that is raised within the domain and can be handled by one or more
/// <see cref="DomainEventHandler{TEvent}"/>. It is published with
/// <see cref="IPublisher.Publish(INotification, CancellationToken)"/>.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>When the event occurred (UTC).</summary>
    public DateTime EventOccurredAtUtc { get; }

    /// <summary>The id of the aggregate that raised the event.</summary>
    /// <remarks>
    /// Lets consumers (logging, auditing, outbox, etc.) know which aggregate the event belongs to
    /// without inspecting the concrete event type.
    /// NOTE: the aggregate id could be a string, Guid, int, etc., so it is exposed as a string and
    /// each event must convert its id to string.
    /// </remarks>
    public string AggregateId { get; }
}
