using Records.Shared.Domain.Events;
using Records.Shared.Mediator.Abstractions;
using Records.Shared.Messaging;

namespace Records.Shared.Application.DomainEvents;

/// <summary>
/// Default <see cref="IDomainEventPublisher"/>: wraps each domain event in a <see cref="DomainMessage{TEvent}"/>
/// and publishes it with <see cref="IPublisher"/>.
/// </summary>
public class DomainEventPublisher : IDomainEventPublisher
{
    #region Declarations

    private readonly IPublisher _publisher;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventPublisher"/> class.
    /// </summary>
    /// <param name="publisher">Publisher used to deliver the messages to their handlers.</param>
    public DomainEventPublisher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    #endregion

    #region Public methods

    /// <inheritdoc/>
    public async Task Publish(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        // Secuencial (no Task.WhenAll): los handlers comparten dependencias Scoped (IDbConnection/UoW de
        // Dapper) que no son thread-safe.
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            MessageMetadata metadata = new(domainEvent.AggregateId);
            INotification message = DomainMessage.Create(metadata, domainEvent);

            await _publisher.Publish(message, cancellationToken);
        }
    }

    #endregion
}
