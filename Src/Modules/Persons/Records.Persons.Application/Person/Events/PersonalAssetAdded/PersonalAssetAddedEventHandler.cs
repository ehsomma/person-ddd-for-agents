using BuildingBlocks.Application.DomainEvents;
using Microsoft.Extensions.Logging;
using Records.Persons.Domain.PersonAggregate.Events;

namespace Records.Persons.Application.Person.Events.PersonalAssetAdded;

/// <summary>
/// The <see cref="PersonalAssetAddedEvent"/> handler.
/// </summary>
internal sealed class PersonalAssetAddedEventHandler : DomainEventHandler<PersonalAssetAddedEvent>
{
    #region Declarations

    private readonly ILogger<PersonalAssetAddedEventHandler> _logger;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonalAssetAddedEventHandler"/> class.
    /// </summary>
    /// <param name="logger">Logger injection.</param>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/>.</exception>
    public PersonalAssetAddedEventHandler(ILogger<PersonalAssetAddedEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public override Task Handle(DomainMessage<PersonalAssetAddedEvent> notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        // From here (event handler) you can:
        // • Publish integration events or save to the Outbox.
        // • Execute atomic logic (UOW/Transaction) idem command handlers.
        // • Create and send other commands.

        // From here (event handler) you can not:
        // • Execute other atomic logic of another aggregate.

        _logger.LogInformation(
            "PersonalAssetAdded handled. PersonId: {PersonId}, PersonalAsset: {PersonalAssetDescription}, MessageId: {MessageId}, CorrelationId: {CorrelationId}",
            notification.Content.Person.Id,
            notification.Content.PersonalAsset.Description.Value,
            notification.Metadata.MessageId,
            notification.Metadata.CorrelationId);

        return Task.CompletedTask;
    }

    #endregion
}
