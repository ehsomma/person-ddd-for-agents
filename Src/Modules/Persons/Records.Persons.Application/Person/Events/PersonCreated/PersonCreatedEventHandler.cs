using BuildingBlocks.Application.DomainEvents;
using Microsoft.Extensions.Logging;
using Records.Persons.Domain.PersonAggregate.Events;

namespace Records.Persons.Application.Person.Events.PersonCreated;

/// <summary>
/// The <see cref="PersonCreatedEvent"/> handler.
/// </summary>
internal sealed class PersonCreatedEventHandler : DomainEventHandler<PersonCreatedEvent>
{
    #region Declarations

    private readonly ILogger<PersonCreatedEventHandler> _logger;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonCreatedEventHandler"/> class.
    /// </summary>
    /// <param name="logger">Logger injection.</param>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/>.</exception>
    public PersonCreatedEventHandler(ILogger<PersonCreatedEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public override Task Handle(DomainMessage<PersonCreatedEvent> notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        // From here (event handler) you can:
        // • Publish integration events or save to the Outbox.
        // • Execute atomic logic (UOW/Transaction) idem command handlers.
        // • Create and send other commands.

        // From here (event handler) you can not:
        // • Execute other atomic logic of another aggregate.

        _logger.LogInformation(
            "PersonCreated handled. PersonId: {PersonId}, MessageId: {MessageId}, CorrelationId: {CorrelationId}",
            notification.Content.Person.Id,
            notification.Metadata.MessageId,
            notification.Metadata.CorrelationId);

        return Task.CompletedTask;
    }

    #endregion
}
