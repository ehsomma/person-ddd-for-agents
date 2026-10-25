using AwesomeAssertions;
using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Application.Mappings.Abstractions.Mappers;
using BuildingBlocks.Application.Persistence.Abstractions;
using BuildingBlocks.Domain.Exceptions;
using Moq;
using Records.Persons.Application.Person.Commands.UpdatePerson;
using Records.Persons.Application.UnitTests.TestData;
using Records.Persons.Domain.PersonAggregate.Errors;
using Records.Persons.Domain.PersonAggregate.Events;
using Records.Persons.Domain.PersonAggregate.Repositories;
using Records.Persons.Domain.PersonAggregate.Services;
using Records.Persons.TestData;
using Xunit;
using DomainModel = Records.Persons.Domain.PersonAggregate.Models; // Using aliases.
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.UnitTests.Person.Commands.UpdatePerson;

/// <summary>
/// Tests del command handler <see cref="UpdatePersonCommandHandler"/>: carga el aggregate actual, lo
/// actualiza con los datos del DTO, lo guarda en una transacción, publica los eventos después del
/// commit y devuelve la persona tal como quedó guardada.
/// </summary>
public sealed class UpdatePersonCommandHandlerTests
{
    #region Declarations

    private readonly CommandHandlerTestContext _context = new CommandHandlerTestContext();

    private readonly Mock<IPersonService> _personServiceMock = new Mock<IPersonService>();

    private readonly Mock<IPersonRepository> _personRepositoryMock = new Mock<IPersonRepository>();

    private readonly Mock<IDomainMapper<DomainModel.Person, Dto.Person>> _personMapperMock =
        new Mock<IDomainMapper<DomainModel.Person, Dto.Person>>();

    private DomainModel.Person? _updatedPerson;

    #endregion

    #region Constructor

    /// <summary>Inicializa una nueva instancia de <see cref="UpdatePersonCommandHandlerTests"/>.</summary>
    public UpdatePersonCommandHandlerTests()
    {
        // Configuración común de todos los tests (xUnit crea una instancia por test): el update guarda
        // el aggregate que recibió para poder revisarlo en el Assert.
        _personRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<DomainModel.Person>(), It.IsAny<CancellationToken>()))
            .Callback<DomainModel.Person, CancellationToken>((person, _) =>
            {
                _context.RecordCall(nameof(IPersonRepository.UpdateAsync));
                _updatedPerson = person;
            })
            .Returns(Task.CompletedTask);
    }

    #endregion

    #region Public methods

    [Fact]
    public async Task Handle_WhenPersonExists_ShouldUpdateTheLoadedAggregateWithTheDtoData()
    {
        // Arrange.
        // Se guarda el aggregate que cargó el servicio, actualizado con Person.Update (no el que se
        // armó con los datos del DTO): así se conserva lo que el DTO no trae, como el ID de la
        // dirección o las fechas de auditoría.
        Guid id = Guid.NewGuid();
        (DomainModel.Person currentPerson, _) = SetupCurrentAndPersistedPerson(id);
        Dto.Person personDto = PersonDtoMother.ValidPerson(id);
        UpdatePersonCommandHandler handler = CreateHandler();

        // Act.
        await handler.Handle(new UpdatePersonCommand(personDto), TestContext.Current.CancellationToken);

        // Assert.
        _updatedPerson.Should().BeSameAs(currentPerson);
        _updatedPerson!.FullName.Value.Should().Be(personDto.FullName);
        _updatedPerson.Email.Value.Should().Be(personDto.Email);
        _updatedPerson.Address.City!.Value.Should().Be(personDto.Address!.City);
        _updatedPerson.Address.Id.Should().Be(7); // El de PersonMother.ExistingPerson.
        _updatedPerson.CreatedOnUtc.Should().Be(PersonMother.CreatedOnUtc);
    }

    [Fact]
    public async Task Handle_WhenDtoHasNewAndExistingPersonalAssets_ShouldAddOnlyTheNewOnes()
    {
        // Arrange.
        // La persona actual tiene el bien 15. El DTO trae ese mismo bien y uno nuevo (sin ID): el
        // handler los carga tal como vienen, y es Person.Update el que decide agregar solo el nuevo.
        Guid id = Guid.NewGuid();
        DomainModel.Person currentPerson = PersonMother.ExistingPerson(
            id,
            personalAssets: new List<DomainModel.PersonalAsset> { PersonMother.ExistingPersonalAsset(15) });
        SetupCurrentAndPersistedPerson(id, currentPerson);
        Dto.Person personDto = PersonDtoMother.ValidPerson(
            id,
            new List<Dto.PersonalAsset>
            {
                PersonDtoMother.PersonalAsset(15, "Bicicleta", 800M),
                PersonDtoMother.PersonalAsset(description: "Notebook"),
            });
        UpdatePersonCommandHandler handler = CreateHandler();

        // Act.
        await handler.Handle(new UpdatePersonCommand(personDto), TestContext.Current.CancellationToken);

        // Assert.
        _updatedPerson!.PersonalAssets.Should().HaveCount(2);
        _updatedPerson.PersonalAssets.Should().ContainSingle(personalAsset => personalAsset.Id == 0)
            .Which.Description.Value.Should().Be("Notebook");
    }

    [Fact]
    public async Task Handle_WhenPersonExists_ShouldPublishEventsAfterCommit()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        SetupCurrentAndPersistedPerson(id);
        UpdatePersonCommandHandler handler = CreateHandler();

        // Act.
        await handler.Handle(
            new UpdatePersonCommand(PersonDtoMother.ValidPerson(id)),
            TestContext.Current.CancellationToken);

        // Assert.
        _context.Calls.Should().ContainInConsecutiveOrder(
            nameof(IUnitOfWork.BeginTransactionAsync),
            nameof(IPersonRepository.UpdateAsync),
            nameof(IUnitOfWork.CommitAsync),
            nameof(IDomainEventPublisher.Publish));
        _context.PublishedEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PersonUpdatedEvent>();
        _context.PublishCancellationToken.Should().Be(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenPersonExists_ShouldReturnThePersonReadAgainAfterCommit()
    {
        // Arrange.
        // El handler vuelve a leer la persona después del commit para devolver los IDs que generó la
        // base (p.ej. los de los bienes personales nuevos). El mapper recibe la persona releída.
        Guid id = Guid.NewGuid();
        (_, DomainModel.Person persistedPerson) = SetupCurrentAndPersistedPerson(id);
        Dto.Person persistedPersonDto = PersonDtoMother.ValidPerson(id);
        _personMapperMock
            .Setup(mapper => mapper.FromDomainToDto(persistedPerson))
            .Returns(persistedPersonDto);
        UpdatePersonCommandHandler handler = CreateHandler();

        // Act.
        Dto.Person result = await handler.Handle(
            new UpdatePersonCommand(PersonDtoMother.ValidPerson(id)),
            TestContext.Current.CancellationToken);

        // Assert.
        result.Should().BeSameAs(persistedPersonDto);
        _context.Calls[^1].Should().Be(nameof(IPersonService.GetByIdAsync));
    }

    [Fact]
    public async Task Handle_WhenPersonDoesNotExist_ShouldThrowNotFoundWithoutOpeningTheTransaction()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        _personServiceMock
            .Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException(DomainErrors.Person.NotFound));
        UpdatePersonCommandHandler handler = CreateHandler();

        // Act.
        Func<Task> act = () => handler.Handle(
            new UpdatePersonCommand(PersonDtoMother.ValidPerson(id)),
            TestContext.Current.CancellationToken);

        // Assert.
        await act.Should().ThrowAsync<DomainException>()
            .Where(exception => exception.Error.Code == DomainErrors.Person.NotFound.Code);
        _context.Calls.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenUpdateFails_ShouldRollbackAndNotPublishEvents()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        SetupCurrentAndPersistedPerson(id);
        _personRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<DomainModel.Person>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error."));
        UpdatePersonCommandHandler handler = CreateHandler();

        // Act.
        Func<Task> act = () => handler.Handle(
            new UpdatePersonCommand(PersonDtoMother.ValidPerson(id)),
            TestContext.Current.CancellationToken);

        // Assert.
        await act.Should().ThrowAsync<InvalidOperationException>();
        _context.Calls.Should().Equal(
            nameof(IPersonService.GetByIdAsync),
            nameof(IUnitOfWork.BeginTransactionAsync),
            nameof(IUnitOfWork.RollbackAsync));
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Crea el handler con los mocks de la clase.
    /// </summary>
    /// <returns>El handler.</returns>
    private UpdatePersonCommandHandler CreateHandler()
    {
        UpdatePersonCommandHandler handler = new UpdatePersonCommandHandler(
            _context.DomainEventPublisherMock.Object,
            _context.UnitOfWorkMock.Object,
            _personServiceMock.Object,
            _personRepositoryMock.Object,
            _personMapperMock.Object);

        return handler;
    }

    /// <summary>
    /// Configura el servicio para las dos lecturas del handler: la primera devuelve la persona actual
    /// (la que se actualiza) y la segunda, la persona tal como quedó guardada después del commit.
    /// </summary>
    /// <param name="id">El ID de la persona.</param>
    /// <param name="currentPerson">La persona actual, o <see langword="null"/> para usar la de <see cref="PersonMother.ExistingPerson"/>.</param>
    /// <returns>La persona actual y la persona guardada.</returns>
    private (DomainModel.Person CurrentPerson, DomainModel.Person PersistedPerson) SetupCurrentAndPersistedPerson(
        Guid id,
        DomainModel.Person? currentPerson = null)
    {
        DomainModel.Person current = currentPerson ?? PersonMother.ExistingPerson(id);
        DomainModel.Person persisted = PersonMother.ExistingPerson(id);

        // Una cola y no SetupSequence: SetupSequence no acepta un Callback, y hace falta para registrar
        // cada GetByIdAsync en el orden de las llamadas. Un segundo Setup sobre la misma expresión no
        // sirve: reemplaza al primero.
        Queue<DomainModel.Person> people = new Queue<DomainModel.Person>(
            new DomainModel.Person[] { current, persisted });
        _personServiceMock
            .Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .Callback(() => _context.RecordCall(nameof(IPersonService.GetByIdAsync)))
            .ReturnsAsync(() => people.Dequeue());

        (DomainModel.Person CurrentPerson, DomainModel.Person PersistedPerson) currentAndPersisted = (current, persisted);

        return currentAndPersisted;
    }

    #endregion
}
