using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using Moq;
using Records.Persons.Domain.PersonAggregate.Errors;
using Records.Persons.Domain.PersonAggregate.Models;
using Records.Persons.Domain.PersonAggregate.Repositories;
using Records.Persons.Domain.PersonAggregate.Services;
using Records.Persons.Domain.UnitTests.TestData;
using Xunit;

namespace Records.Persons.Domain.UnitTests.PersonAggregate.Services;

/// <summary>
/// Tests del servicio de dominio <see cref="PersonService"/>, con el repositorio
/// (<see cref="IPersonRepository"/>) mockeado.
/// </summary>
public sealed class PersonServiceTests
{
    #region Declarations

    // xUnit crea una instancia nueva de la clase por cada test, así que cada test arranca con un
    // mock limpio, sin los Setup de los otros tests.
    private readonly Mock<IPersonRepository> _personRepositoryMock = new Mock<IPersonRepository>();

    #endregion

    #region Public methods

    [Fact]
    public async Task GetByIdAsync_WhenPersonExists_ShouldReturnPerson()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        Person existingPerson = PersonMother.ExistingPerson(id);
        _personRepositoryMock
            .Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPerson);

        PersonService personService = new PersonService(_personRepositoryMock.Object);

        // Act.
        Person person = await personService.GetByIdAsync(id, TestContext.Current.CancellationToken);

        // Assert.
        person.Should().BeSameAs(existingPerson);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPersonDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        _personRepositoryMock
            .Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        PersonService personService = new PersonService(_personRepositoryMock.Object);

        // Act.
        Func<Task> act = () => personService.GetByIdAsync(id, TestContext.Current.CancellationToken);

        // Assert.
        // Se compara el código del error y no el mensaje: el código es el contrato (es lo que viaja
        // en el ErrorResponse) y el mensaje puede cambiar.
        await act.Should().ThrowAsync<DomainException>()
            .Where(exception => exception.Error.Code == DomainErrors.Person.NotFound.Code);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCalled_ShouldPassCancellationTokenToRepository()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        _personRepositoryMock
            .Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PersonMother.ExistingPerson(id));

        PersonService personService = new PersonService(_personRepositoryMock.Object);

        // Act.
        await personService.GetByIdAsync(id, cancellationToken);

        // Assert.
        // El CancellationToken tiene que llegar hasta el repositorio (ver CLAUDE.md).
        _personRepositoryMock.Verify(
            repository => repository.GetByIdAsync(id, cancellationToken),
            Times.Once);
    }

    [Fact]
    public void Constructor_WhenRepositoryIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange.

        // Act.
        Action act = () => _ = new PersonService(null!);

        // Assert.
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("repository");
    }

    #endregion
}
