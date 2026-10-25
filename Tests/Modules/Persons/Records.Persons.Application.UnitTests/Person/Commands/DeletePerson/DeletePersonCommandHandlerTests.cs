using AwesomeAssertions;
using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Application.Persistence.Abstractions;
using BuildingBlocks.Domain.Exceptions;
using Moq;
using Records.Persons.Application.Person.Commands.DeletePerson;
using Records.Persons.Application.UnitTests.TestData;
using Records.Persons.Domain.PersonAggregate.Errors;
using Records.Persons.Domain.PersonAggregate.Events;
using Records.Persons.Domain.PersonAggregate.Repositories;
using Records.Persons.Domain.PersonAggregate.Services;
using Records.Persons.TestData;
using Xunit;
using DomainModel = Records.Persons.Domain.PersonAggregate.Models; // Using aliases.

namespace Records.Persons.Application.UnitTests.Person.Commands.DeletePerson;

/// <summary>
/// Tests del command handler <see cref="DeletePersonCommandHandler"/>: carga el aggregate, lo elimina
/// en una transacción y publica el <see cref="PersonDeletedEvent"/> después del commit.
/// </summary>
public sealed class DeletePersonCommandHandlerTests
{
    #region Declarations

    private readonly CommandHandlerTestContext _context = new CommandHandlerTestContext();

    private readonly Mock<IPersonService> _personServiceMock = new Mock<IPersonService>();

    private readonly Mock<IPersonRepository> _personRepositoryMock = new Mock<IPersonRepository>();

    #endregion

    #region Public methods

    [Fact]
    public async Task Handle_WhenPersonExists_ShouldDeleteTheLoadedAggregate()
    {
        // Arrange.
        // Se elimina el aggregate que cargó el servicio (no uno armado con el ID del command): es el
        // que pasó por Person.Delete y tiene registrado el evento.
        Guid id = Guid.NewGuid();
        DomainModel.Person person = SetupExistingPerson(id);
        SetupDeleteAsync();
        DeletePersonCommandHandler handler = CreateHandler();

        // Act.
        await handler.Handle(new DeletePersonCommand(id), TestContext.Current.CancellationToken);

        // Assert.
        _personRepositoryMock.Verify(
            repository => repository.DeleteAsync(person, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPersonExists_ShouldPublishPersonDeletedEventAfterCommit()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        DomainModel.Person person = SetupExistingPerson(id);
        SetupDeleteAsync();
        DeletePersonCommandHandler handler = CreateHandler();

        // Act.
        await handler.Handle(new DeletePersonCommand(id), TestContext.Current.CancellationToken);

        // Assert.
        // Los handlers de los eventos solo tienen que reaccionar a cambios ya persistidos: el Publish
        // va después del CommitAsync, nunca dentro de la transacción.
        _context.Calls.Should().Equal(
            nameof(IUnitOfWork.BeginTransactionAsync),
            nameof(IPersonRepository.DeleteAsync),
            nameof(IUnitOfWork.CommitAsync),
            nameof(IDomainEventPublisher.Publish));
        _context.PublishedEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PersonDeletedEvent>()
            .Which.Person.Should().BeSameAs(person);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldPublishEventsWithoutTheRequestCancellationToken()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        SetupExistingPerson(id);
        SetupDeleteAsync();
        DeletePersonCommandHandler handler = CreateHandler();
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        // Act.
        await handler.Handle(new DeletePersonCommand(id), cancellationTokenSource.Token);

        // Assert.
        // Lo ya commiteado no se cancela: si el cliente se desconecta después del commit, los
        // eventos no pueden quedar publicados a medias (ver CLAUDE.md).
        _context.PublishCancellationToken.Should().Be(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldPassCancellationTokenToServiceAndRepository()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        SetupExistingPerson(id);
        SetupDeleteAsync();
        DeletePersonCommandHandler handler = CreateHandler();
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act.
        await handler.Handle(new DeletePersonCommand(id), cancellationToken);

        // Assert.
        _personServiceMock.Verify(service => service.GetByIdAsync(id, cancellationToken), Times.Once);
        _personRepositoryMock.Verify(
            repository => repository.DeleteAsync(It.IsAny<DomainModel.Person>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPersonDoesNotExist_ShouldThrowNotFoundWithoutOpeningTheTransaction()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        _personServiceMock
            .Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException(DomainErrors.Person.NotFound));
        DeletePersonCommandHandler handler = CreateHandler();

        // Act.
        Func<Task> act = () => handler.Handle(new DeletePersonCommand(id), TestContext.Current.CancellationToken);

        // Assert.
        await act.Should().ThrowAsync<DomainException>()
            .Where(exception => exception.Error.Code == DomainErrors.Person.NotFound.Code);
        _context.Calls.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenDeleteFails_ShouldRollbackAndNotPublishEvents()
    {
        // Arrange.
        // Si el repositorio falla, la transacción se deshace y no se publica nada: un evento de algo
        // que no se guardó haría reaccionar a los handlers sobre datos que no existen.
        Guid id = Guid.NewGuid();
        SetupExistingPerson(id);
        _personRepositoryMock
            .Setup(repository => repository.DeleteAsync(It.IsAny<DomainModel.Person>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error."));
        DeletePersonCommandHandler handler = CreateHandler();

        // Act.
        Func<Task> act = () => handler.Handle(new DeletePersonCommand(id), TestContext.Current.CancellationToken);

        // Assert.
        await act.Should().ThrowAsync<InvalidOperationException>();
        _context.Calls.Should().Equal(
            nameof(IUnitOfWork.BeginTransactionAsync),
            nameof(IUnitOfWork.RollbackAsync));
    }

    [Fact]
    public async Task Handle_WhenCancellationIsRequested_ShouldNotOpenTheTransaction()
    {
        // Arrange.
        // El token se chequea antes de abrir la transacción: si ya está cancelado, no hay nada que
        // deshacer.
        Guid id = Guid.NewGuid();
        SetupExistingPerson(id);
        DeletePersonCommandHandler handler = CreateHandler();
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        // Act.
        Func<Task> act = () => handler.Handle(new DeletePersonCommand(id), cancellationTokenSource.Token);

        // Assert.
        await act.Should().ThrowAsync<OperationCanceledException>();
        _context.Calls.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCommandIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange.
        DeletePersonCommandHandler handler = CreateHandler();

        // Act.
        Func<Task> act = () => handler.Handle(null!, TestContext.Current.CancellationToken);

        // Assert.
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Crea el handler con los mocks de la clase.
    /// </summary>
    /// <returns>El handler.</returns>
    private DeletePersonCommandHandler CreateHandler()
    {
        DeletePersonCommandHandler handler = new DeletePersonCommandHandler(
            _context.DomainEventPublisherMock.Object,
            _context.UnitOfWorkMock.Object,
            _personServiceMock.Object,
            _personRepositoryMock.Object);

        return handler;
    }

    /// <summary>
    /// Configura el servicio para que devuelva una persona existente con el <paramref name="id"/>
    /// especificado.
    /// </summary>
    /// <param name="id">El ID de la persona.</param>
    /// <returns>La persona que devuelve el servicio.</returns>
    private DomainModel.Person SetupExistingPerson(Guid id)
    {
        DomainModel.Person person = PersonMother.ExistingPerson(id);
        _personServiceMock
            .Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);

        return person;
    }

    /// <summary>
    /// Configura el repositorio para que registre el <c>DeleteAsync</c> en el orden de las llamadas.
    /// </summary>
    private void SetupDeleteAsync()
    {
        _personRepositoryMock
            .Setup(repository => repository.DeleteAsync(It.IsAny<DomainModel.Person>(), It.IsAny<CancellationToken>()))
            .Callback(() => _context.RecordCall(nameof(IPersonRepository.DeleteAsync)))
            .Returns(Task.CompletedTask);
    }

    #endregion
}
