using AwesomeAssertions;
using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Application.Mappings.Abstractions.Mappers;
using BuildingBlocks.Application.Persistence.Abstractions;
using BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.Options;
using Moq;
using Records.Persons.Application.Person.Commands.CreatePerson;
using Records.Persons.Application.UnitTests.TestData;
using Records.Persons.Configuration;
using Records.Persons.Domain.PersonAggregate.Enumerators;
using Records.Persons.Domain.PersonAggregate.Events;
using Records.Persons.Domain.PersonAggregate.Repositories;
using Records.Persons.Domain.PersonAggregate.Services;
using Records.Persons.TestData;
using Xunit;
using DomainModel = Records.Persons.Domain.PersonAggregate.Models; // Using aliases.
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.UnitTests.Person.Commands.CreatePerson;

/// <summary>
/// Tests del command handler <see cref="CreatePersonCommandHandler"/>: arma el aggregate a partir del
/// DTO, lo inserta en una transacción, publica los eventos después del commit y devuelve la persona
/// tal como quedó guardada.
/// </summary>
public sealed class CreatePersonCommandHandlerTests
{
    #region Declarations

    private readonly CommandHandlerTestContext _context = new CommandHandlerTestContext();

    private readonly Mock<IOptionsSnapshot<PersonsSettings>> _settingsMock = new Mock<IOptionsSnapshot<PersonsSettings>>();

    private readonly Mock<IPersonRepository> _personRepositoryMock = new Mock<IPersonRepository>();

    private readonly Mock<IPersonService> _personServiceMock = new Mock<IPersonService>();

    private readonly Mock<IDomainMapper<DomainModel.Person, Dto.Person>> _personMapperMock =
        new Mock<IDomainMapper<DomainModel.Person, Dto.Person>>();

    private DomainModel.Person? _insertedPerson;

    #endregion

    #region Constructor

    /// <summary>Inicializa una nueva instancia de <see cref="CreatePersonCommandHandlerTests"/>.</summary>
    public CreatePersonCommandHandlerTests()
    {
        // Configuración común de todos los tests (xUnit crea una instancia por test): el handler
        // recibe settings aunque hoy no los usa, y el insert guarda el aggregate que recibió para
        // poder revisarlo en el Assert.
        _settingsMock.Setup(settings => settings.Value).Returns(new PersonsSettings());

        _personRepositoryMock
            .Setup(repository => repository.InsertAsync(It.IsAny<DomainModel.Person>(), It.IsAny<CancellationToken>()))
            .Callback<DomainModel.Person, CancellationToken>((person, _) =>
            {
                _context.RecordCall(nameof(IPersonRepository.InsertAsync));
                _insertedPerson = person;
            })
            .Returns(Task.CompletedTask);
    }

    #endregion

    #region Public methods

    [Fact]
    public async Task Handle_WhenDataIsValid_ShouldInsertPersonBuiltFromTheDto()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        Dto.Person personDto = PersonDtoMother.ValidPerson(id);
        SetupPersistedPerson(id);
        CreatePersonCommandHandler handler = CreateHandler();

        // Act.
        await handler.Handle(new CreatePersonCommand(personDto), TestContext.Current.CancellationToken);

        // Assert.
        _insertedPerson.Should().NotBeNull();
        _insertedPerson!.Id.Should().Be(id);
        _insertedPerson.FullName.Value.Should().Be(personDto.FullName);
        _insertedPerson.Email.Value.Should().Be(personDto.Email);
        _insertedPerson.Phone!.Value.Should().Be(personDto.Phone);
        _insertedPerson.Gender.Should().Be(Gender.Female);
        _insertedPerson.Birthdate.Should().Be(personDto.Birthdate);
        _insertedPerson.Address.Id.Should().Be(0);
        _insertedPerson.Address.City!.Value.Should().Be(personDto.Address!.City);
        _insertedPerson.Address.LatLng!.Lat.Should().Be(personDto.Address.LatLng!.Lat);
    }

    [Fact]
    public async Task Handle_WhenDtoHasPersonalAssets_ShouldInsertThemAsNew()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        Dto.Person personDto = PersonDtoMother.ValidPerson(
            id,
            new List<Dto.PersonalAsset>
            {
                PersonDtoMother.PersonalAsset(description: "Notebook"),
                PersonDtoMother.PersonalAsset(description: "Celular"),
            });
        SetupPersistedPerson(id);
        CreatePersonCommandHandler handler = CreateHandler();

        // Act.
        await handler.Handle(new CreatePersonCommand(personDto), TestContext.Current.CancellationToken);

        // Assert.
        _insertedPerson!.PersonalAssets.Select(personalAsset => personalAsset.Description.Value)
            .Should().Equal("Notebook", "Celular");
        _insertedPerson.PersonalAssets.Should().AllSatisfy(personalAsset => personalAsset.Id.Should().Be(0));
    }

    [Fact]
    public async Task Handle_WhenDataIsValid_ShouldPublishEventsAfterCommit()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        Dto.Person personDto = PersonDtoMother.ValidPerson(
            id,
            new List<Dto.PersonalAsset> { PersonDtoMother.PersonalAsset() });
        SetupPersistedPerson(id);
        CreatePersonCommandHandler handler = CreateHandler();

        // Act.
        await handler.Handle(new CreatePersonCommand(personDto), TestContext.Current.CancellationToken);

        // Assert.
        // Se publican todos los eventos que registró el aggregate, en orden: el de la creación y uno
        // por cada bien personal agregado.
        _context.Calls.Should().ContainInConsecutiveOrder(
            nameof(IUnitOfWork.BeginTransactionAsync),
            nameof(IPersonRepository.InsertAsync),
            nameof(IUnitOfWork.CommitAsync),
            nameof(IDomainEventPublisher.Publish));
        _context.PublishedEvents.Should().HaveCount(2);
        _context.PublishedEvents[0].Should().BeOfType<PersonCreatedEvent>();
        _context.PublishedEvents[^1].Should().BeOfType<PersonalAssetAddedEvent>();
        _context.PublishCancellationToken.Should().Be(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenDataIsValid_ShouldReturnThePersonReadAgainAfterCommit()
    {
        // Arrange.
        // El handler no devuelve el aggregate que insertó: lo vuelve a leer, porque el insert no le
        // devuelve los IDs generados (la dirección y los bienes personales). El mapper recibe la
        // persona leída, no la insertada.
        Guid id = Guid.NewGuid();
        DomainModel.Person persistedPerson = SetupPersistedPerson(id);
        Dto.Person persistedPersonDto = PersonDtoMother.ValidPerson(id);
        _personMapperMock
            .Setup(mapper => mapper.FromDomainToDto(persistedPerson))
            .Returns(persistedPersonDto);
        CreatePersonCommandHandler handler = CreateHandler();

        // Act.
        Dto.Person result = await handler.Handle(
            new CreatePersonCommand(PersonDtoMother.ValidPerson(id)),
            TestContext.Current.CancellationToken);

        // Assert.
        result.Should().BeSameAs(persistedPersonDto);
        _context.Calls[^1].Should().Be(nameof(IPersonService.GetByIdAsync));
    }

    [Fact]
    public async Task Handle_WhenInsertFails_ShouldRollbackAndNotPublishEvents()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        _personRepositoryMock
            .Setup(repository => repository.InsertAsync(It.IsAny<DomainModel.Person>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error."));
        CreatePersonCommandHandler handler = CreateHandler();

        // Act.
        Func<Task> act = () => handler.Handle(
            new CreatePersonCommand(PersonDtoMother.ValidPerson(id)),
            TestContext.Current.CancellationToken);

        // Assert.
        await act.Should().ThrowAsync<InvalidOperationException>();
        _context.Calls.Should().Equal(
            nameof(IUnitOfWork.BeginTransactionAsync),
            nameof(IUnitOfWork.RollbackAsync));
    }

    [Fact]
    public async Task Handle_WhenDtoIsInvalid_ShouldThrowDomainValidationExceptionWithoutOpeningTheTransaction()
    {
        // Arrange.
        // El validator de la API ya rechaza un género inexistente, pero el dominio no confía en eso:
        // Gender.FromName lanza la excepción antes de tocar la base.
        Dto.Person personDto = new Dto.Person
        {
            Id = Guid.NewGuid(),
            FullName = "Roe, Jane",
            Email = "janeroe@gmail.com",
            Gender = "Unknown",
            Address = PersonDtoMother.ValidPerson(Guid.NewGuid()).Address,
        };
        CreatePersonCommandHandler handler = CreateHandler();

        // Act.
        Func<Task> act = () => handler.Handle(new CreatePersonCommand(personDto), TestContext.Current.CancellationToken);

        // Assert.
        await act.Should().ThrowAsync<DomainValidationException>();
        _context.Calls.Should().BeEmpty();
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Crea el handler con los mocks de la clase.
    /// </summary>
    /// <returns>El handler.</returns>
    private CreatePersonCommandHandler CreateHandler()
    {
        CreatePersonCommandHandler handler = new CreatePersonCommandHandler(
            _context.DomainEventPublisherMock.Object,
            _context.UnitOfWorkMock.Object,
            _settingsMock.Object,
            _personRepositoryMock.Object,
            _personServiceMock.Object,
            _personMapperMock.Object);

        return handler;
    }

    /// <summary>
    /// Configura el servicio para que, al releer la persona después del commit, devuelva una persona
    /// existente con el <paramref name="id"/> especificado (como la devolvería la base, con los IDs
    /// generados).
    /// </summary>
    /// <param name="id">El ID de la persona.</param>
    /// <returns>La persona que devuelve el servicio.</returns>
    private DomainModel.Person SetupPersistedPerson(Guid id)
    {
        DomainModel.Person persistedPerson = PersonMother.ExistingPerson(id);
        _personServiceMock
            .Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .Callback(() => _context.RecordCall(nameof(IPersonService.GetByIdAsync)))
            .ReturnsAsync(persistedPerson);

        return persistedPerson;
    }

    #endregion
}
