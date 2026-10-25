using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Application.Persistence.Abstractions;
using BuildingBlocks.Domain.Events;
using Moq;

namespace Records.Persons.Application.UnitTests.TestData;

/// <summary>
/// Arma los mocks que todo command handler recibe de su base (<c>CommandHandler</c>): el
/// <see cref="IUnitOfWork"/> y el <see cref="IDomainEventPublisher"/>, y registra en orden las
/// llamadas que reciben.
/// <para>
/// El orden es lo que se testea: los eventos se publican recién después del commit, y si la operación
/// falla se hace rollback y no se publica nada. Los mocks del repositorio de cada test también
/// registran sus llamadas con <see cref="RecordCall"/>, así el orden completo queda en
/// <see cref="Calls"/>, por ejemplo:
/// <c>BeginTransactionAsync → DeleteAsync → CommitAsync → Publish</c>.
/// </para>
/// </summary>
internal sealed class CommandHandlerTestContext
{
    #region Declarations

    private readonly List<string> _calls = new List<string>();

    private readonly List<IDomainEvent> _publishedEvents = new List<IDomainEvent>();

    #endregion

    #region Constructor

    /// <summary>Inicializa una nueva instancia de <see cref="CommandHandlerTestContext"/>.</summary>
    public CommandHandlerTestContext()
    {
        UnitOfWorkMock
            .Setup(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Callback(() => RecordCall(nameof(IUnitOfWork.BeginTransactionAsync)))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CommitAsync())
            .Callback(() => RecordCall(nameof(IUnitOfWork.CommitAsync)))
            .Returns(Task.CompletedTask);

        UnitOfWorkMock
            .Setup(unitOfWork => unitOfWork.RollbackAsync())
            .Callback(() => RecordCall(nameof(IUnitOfWork.RollbackAsync)))
            .Returns(Task.CompletedTask);

        // Además del orden, se guardan los eventos y el token con los que se publicó: los eventos se
        // publican sin el CancellationToken del request (ver CLAUDE.md).
        DomainEventPublisherMock
            .Setup(publisher => publisher.Publish(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<IDomainEvent>, CancellationToken>((domainEvents, cancellationToken) =>
            {
                RecordCall(nameof(IDomainEventPublisher.Publish));
                _publishedEvents.AddRange(domainEvents);
                PublishCancellationToken = cancellationToken;
            })
            .Returns(Task.CompletedTask);
    }

    #endregion

    #region Properties

    /// <summary>El mock del unit of work.</summary>
    public Mock<IUnitOfWork> UnitOfWorkMock { get; } = new Mock<IUnitOfWork>();

    /// <summary>El mock del publicador de domain events.</summary>
    public Mock<IDomainEventPublisher> DomainEventPublisherMock { get; } = new Mock<IDomainEventPublisher>();

    /// <summary>Las llamadas que recibieron los mocks, en el orden en que se hicieron.</summary>
    public IReadOnlyList<string> Calls => _calls;

    /// <summary>Los domain events que se publicaron.</summary>
    public IReadOnlyList<IDomainEvent> PublishedEvents => _publishedEvents;

    /// <summary>El token con el que se publicaron los eventos, o <see langword="null"/> si no se publicaron.</summary>
    public CancellationToken? PublishCancellationToken { get; private set; }

    #endregion

    #region Public methods

    /// <summary>
    /// Registra una llamada, para los mocks que arma cada test (p.ej. el repositorio).
    /// </summary>
    /// <param name="call">El nombre del método llamado (con <c>nameof</c>).</param>
    public void RecordCall(string call)
    {
        _calls.Add(call);
    }

    #endregion
}
