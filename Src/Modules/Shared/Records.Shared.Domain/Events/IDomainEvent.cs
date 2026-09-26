namespace Records.Shared.Domain.Events;

/// <summary>
/// Represents an event that is raised within the domain.
/// </summary>
/// <remarks>
/// The domain knows nothing about how events are published: the application layer wraps each event in
/// a message (with its metadata) and publishes it.
/// </remarks>
public interface IDomainEvent
{
    /// <summary>When the event occurred (UTC).</summary>
    public DateTime EventOccurredAtUtc { get; }

    /// <summary>The id of the aggregate that raised the event.</summary>
    /// <remarks>
    /// Lets consumers (logging, auditing, message metadata, etc.) know which aggregate the event
    /// belongs to without inspecting the concrete event type.
    /// NOTE: the aggregate id could be a string, Guid, int, etc., so it is exposed as a string and
    /// each event must convert its id to string.
    /// </remarks>
    public string AggregateId { get; }
}
