namespace Records.Shared.Domain.Events;

/// <summary>
/// Base class for events raised within the domain. Sets <see cref="EventOccurredAtUtc"/> to the moment
/// the event is created.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEvent"/> class.
    /// </summary>
    /// <param name="aggregateId"><inheritdoc cref="AggregateId" path="/summary"/></param>
    protected DomainEvent(string aggregateId)
    {
        AggregateId = aggregateId;
        EventOccurredAtUtc = DateTime.UtcNow;
    }

    /// <inheritdoc/>
    public DateTime EventOccurredAtUtc { get; }

    /// <inheritdoc/>
    public string AggregateId { get; }
}
