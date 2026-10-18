using BuildingBlocks.Application.DomainEvents;
using Microsoft.Extensions.Logging;
using Records.Persons.Domain.PersonAggregate.Events;

namespace Records.Persons.Application.Person.Events.PersonDeleted;

/// <summary>
/// Representa el handler del <see cref="PersonDeletedEvent"/>.
/// </summary>
/// <remarks>
/// Se ejecuta después del commit del command que eliminó la persona, así que el borrado ya está
/// persistido cuando llega acá.
/// </remarks>
internal sealed class PersonDeletedEventHandler : DomainEventHandler<PersonDeletedEvent>
{
    #region Declarations

    private readonly ILogger<PersonDeletedEventHandler> _logger;

    #endregion

    #region Constructor

    /// <summary>Inicializa una nueva instancia de <see cref="PersonDeletedEventHandler"/>.</summary>
    /// <param name="logger">El logger.</param>
    /// <exception cref="ArgumentNullException">Cuando alguno de los parámetros es <see langword="null"/>.</exception>
    public PersonDeletedEventHandler(ILogger<PersonDeletedEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public override Task Handle(DomainMessage<PersonDeletedEvent> notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        // Desde un event handler se puede:
        // • Publicar integration events o guardarlos en el Outbox.
        // • Ejecutar lógica atómica (UoW/transacción), igual que en los command handlers.
        // • Crear y enviar otros commands.

        // Desde un event handler no se puede:
        // • Ejecutar lógica atómica de otro aggregate.

        _logger.LogInformation(
            "PersonDeleted handled. PersonId: {PersonId}, MessageId: {MessageId}, CorrelationId: {CorrelationId}",
            notification.Content.Person.Id,
            notification.Metadata.MessageId,
            notification.Metadata.CorrelationId);

        return Task.CompletedTask;
    }

    #endregion
}
