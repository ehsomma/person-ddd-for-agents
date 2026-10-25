using AwesomeAssertions;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.ValueObjects;
using Records.Persons.Domain.PersonAggregate.Enumerators;
using Records.Persons.Domain.PersonAggregate.Events;
using Records.Persons.Domain.PersonAggregate.Models;
using Records.Persons.Domain.PersonAggregate.ValueObjects;
using Records.Persons.TestData;
using Xunit;

namespace Records.Persons.Domain.UnitTests.PersonAggregate.Models;

/// <summary>
/// Tests del aggregate root <see cref="Person"/>: la creación y la carga (<c>Create</c> y
/// <c>Load</c>), las invariantes y los domain events que registra cada método de comportamiento.
/// </summary>
public sealed class PersonTests
{
    #region Public methods

    [Fact]
    public void Create_WhenDataIsValid_ShouldSetAllProperties()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        Address address = PersonMother.NewAddress();
        DateTime birthdate = new DateTime(1985, 3, 10, 0, 0, 0, DateTimeKind.Utc);

        // Act.
        Person person = Person.Create(
            id,
            address,
            FullName.Build("Doe, John"),
            Email.Build("johndoe@gmail.com"),
            PhoneNumber.Build("+54 11 4444-5555"),
            Gender.Male,
            null,
            birthdate);

        // Assert.
        person.Id.Should().Be(id);
        person.Address.Should().BeSameAs(address);
        person.FullName.Value.Should().Be("Doe, John");
        person.Email.Value.Should().Be("johndoe@gmail.com");
        person.Phone!.Value.Should().Be("+54 11 4444-5555");
        person.Gender.Should().Be(Gender.Male);
        person.Birthdate.Should().Be(birthdate);
    }

    [Fact]
    public void Create_WhenDataIsValid_ShouldRegisterPersonCreatedEvent()
    {
        // Arrange.

        // Act.
        Person person = PersonMother.NewPerson();

        // Assert.
        IReadOnlyCollection<IDomainEvent> domainEvents = person.PullDomainEvents();
        domainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PersonCreatedEvent>()
            .Which.Person.Should().BeSameAs(person);
    }

    [Fact]
    public void Create_WhenDataIsValid_ShouldSetCreatedAndUpdatedDatesToNow()
    {
        // Arrange.
        DateTime before = DateTime.UtcNow;

        // Act.
        Person person = PersonMother.NewPerson();

        // Assert.
        // Create toma DateTime.UtcNow, así que no se puede comparar con un valor exacto: se
        // verifica que caiga entre el antes y el después del Act, y que las dos fechas sean la
        // misma (una persona recién creada todavía no se modificó).
        DateTime after = DateTime.UtcNow;
        person.CreatedOnUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        person.UpdatedOnUtc.Should().Be(person.CreatedOnUtc);
    }

    [Fact]
    public void Create_WhenPersonalAssetsIsNull_ShouldHaveNoPersonalAssets()
    {
        // Arrange.

        // Act.
        Person person = PersonMother.NewPerson();

        // Assert.
        person.PersonalAssets.Should().BeEmpty();
    }

    [Fact]
    public void Create_WhenPhoneIsNull_ShouldCreatePerson()
    {
        // Arrange.

        // Act.
        Person person = Person.Create(
            Guid.NewGuid(),
            PersonMother.NewAddress(),
            FullName.Build("Doe, John"),
            Email.Build("johndoe@gmail.com"),
            null, // El teléfono es opcional.
            Gender.Male,
            null,
            null);

        // Assert.
        person.Phone.Should().BeNull();
    }

    [Fact]
    public void Create_WhenAddressIsNull_ShouldThrowDomainValidationException()
    {
        // Arrange.

        // Act.
        Action act = () => Person.Create(
            Guid.NewGuid(),
            null,
            FullName.Build("Doe, John"),
            Email.Build("johndoe@gmail.com"),
            null,
            Gender.Male,
            null,
            null);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Create_WhenFullNameIsNull_ShouldThrowDomainValidationException()
    {
        // Arrange.

        // Act.
        Action act = () => Person.Create(
            Guid.NewGuid(),
            PersonMother.NewAddress(),
            null,
            Email.Build("johndoe@gmail.com"),
            null,
            Gender.Male,
            null,
            null);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Create_WhenEmailIsNull_ShouldThrowDomainValidationException()
    {
        // Arrange.

        // Act.
        Action act = () => Person.Create(
            Guid.NewGuid(),
            PersonMother.NewAddress(),
            FullName.Build("Doe, John"),
            null,
            null,
            Gender.Male,
            null,
            null);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Create_WhenGenderIsNull_ShouldThrowDomainValidationException()
    {
        // Arrange.

        // Act.
        Action act = () => Person.Create(
            Guid.NewGuid(),
            PersonMother.NewAddress(),
            FullName.Build("Doe, John"),
            Email.Build("johndoe@gmail.com"),
            null,
            null,
            null,
            null);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Load_WhenDataIsValid_ShouldNotRegisterDomainEvents()
    {
        // Arrange.

        // Act.
        Person person = PersonMother.ExistingPerson(Guid.NewGuid());

        // Assert.
        person.PullDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void Load_WhenAuditDatesAreSpecified_ShouldKeepThem()
    {
        // Arrange.

        // Act.
        Person person = PersonMother.ExistingPerson(Guid.NewGuid());

        // Assert.
        person.CreatedOnUtc.Should().Be(PersonMother.CreatedOnUtc);
        person.UpdatedOnUtc.Should().Be(PersonMother.UpdatedOnUtc);
    }

    [Fact]
    public void Update_WhenDataIsValid_ShouldReplacePersonalData()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        Person person = PersonMother.ExistingPerson(id);
        Person editedPerson = PersonMother.EditedPerson(id);

        // Act.
        person.Update(editedPerson);

        // Assert.
        person.FullName.Should().Be(editedPerson.FullName);
        person.Email.Should().Be(editedPerson.Email);
        person.Phone.Should().Be(editedPerson.Phone);
        person.Gender.Should().Be(editedPerson.Gender);
        person.Birthdate.Should().Be(editedPerson.Birthdate);
        person.Address.StreetLine1.Should().Be(editedPerson.Address.StreetLine1);
        person.Address.City.Should().Be(editedPerson.Address.City);
        person.Address.LatLng.Should().Be(editedPerson.Address.LatLng);
    }

    [Fact]
    public void Update_WhenAddressComesWithoutId_ShouldKeepCurrentAddressId()
    {
        // Arrange.
        // La dirección editada viene sin ID (Id = 0), como llega desde el body del request. El
        // aggregate tiene que conservar el ID de la dirección actual: es la misma dirección con otros
        // datos, no una nueva.
        Guid id = Guid.NewGuid();
        Person person = PersonMother.ExistingPerson(id, addressId: 7);
        Person editedPerson = PersonMother.EditedPerson(id);

        // Act.
        person.Update(editedPerson);

        // Assert.
        person.Address.Id.Should().Be(7);
    }

    [Fact]
    public void Update_WhenDataIsValid_ShouldRefreshUpdatedOnUtcAndKeepCreatedOnUtc()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        Person person = PersonMother.ExistingPerson(id);
        Person editedPerson = PersonMother.EditedPerson(id);
        DateTime before = DateTime.UtcNow;

        // Act.
        person.Update(editedPerson);

        // Assert.
        person.UpdatedOnUtc.Should().BeOnOrAfter(before);
        person.CreatedOnUtc.Should().Be(PersonMother.CreatedOnUtc);
    }

    [Fact]
    public void Update_WhenDataIsValid_ShouldRegisterPersonUpdatedEvent()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        Person person = PersonMother.ExistingPerson(id);
        Person editedPerson = PersonMother.EditedPerson(id);

        // Act.
        person.Update(editedPerson);

        // Assert.
        IReadOnlyCollection<IDomainEvent> domainEvents = person.PullDomainEvents();
        domainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PersonUpdatedEvent>()
            .Which.Person.Should().BeSameAs(person);
    }

    [Fact]
    public void Update_WhenEditedDataHasNewAndExistingPersonalAssets_ShouldAddOnlyTheNewOnes()
    {
        // Arrange.
        // La persona tiene un bien existente (ID 15). Los datos editados traen ese mismo bien con
        // otra descripción y uno nuevo (sin ID). Update solo agrega el nuevo: el existente no se
        // pisa con los datos que vienen.
        //   Id = 15 → ya existe: queda "Bicicleta" ✓
        //   Id = 0  → nuevo: se agrega "Notebook" ✓
        Guid id = Guid.NewGuid();
        Person person = PersonMother.ExistingPerson(
            id,
            personalAssets: new List<PersonalAsset> { PersonMother.ExistingPersonalAsset(15, "Bicicleta") });

        PersonalAsset newPersonalAsset = PersonMother.NewPersonalAsset("Notebook");
        Person editedPerson = PersonMother.EditedPerson(
            id,
            new List<PersonalAsset>
            {
                PersonMother.ExistingPersonalAsset(15, "Bicicleta de carrera"),
                newPersonalAsset,
            });

        // Act.
        person.Update(editedPerson);

        // Assert.
        person.PersonalAssets.Should().HaveCount(2);
        person.PersonalAssets.Should().ContainSingle(personalAsset => personalAsset.Id == 15)
            .Which.Description.Value.Should().Be("Bicicleta");
        person.PersonalAssets.Should().Contain(newPersonalAsset);
    }

    [Fact]
    public void Update_WhenEditedDataHasNewPersonalAssets_ShouldRegisterOnePersonalAssetAddedEventPerNewAsset()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        Person person = PersonMother.ExistingPerson(id);
        Person editedPerson = PersonMother.EditedPerson(
            id,
            new List<PersonalAsset>
            {
                PersonMother.ExistingPersonalAsset(15),
                PersonMother.NewPersonalAsset("Notebook"),
                PersonMother.NewPersonalAsset("Celular"),
            });

        // Act.
        person.Update(editedPerson);

        // Assert.
        // Primero el PersonUpdatedEvent y después un PersonalAssetAddedEvent por cada bien nuevo (el
        // existente, ID 15, no registra nada).
        IReadOnlyCollection<IDomainEvent> domainEvents = person.PullDomainEvents();
        domainEvents.Should().HaveCount(3);
        domainEvents.First().Should().BeOfType<PersonUpdatedEvent>();
        domainEvents.Skip(1).Should().AllBeOfType<PersonalAssetAddedEvent>();
    }

    [Fact]
    public void Update_WhenEditedDataIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange.
        Person person = PersonMother.ExistingPerson(Guid.NewGuid());

        // Act.
        Action act = () => person.Update(null!);

        // Assert.
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Delete_WhenCalled_ShouldRegisterPersonDeletedEvent()
    {
        // Arrange.
        Person person = PersonMother.ExistingPerson(Guid.NewGuid());

        // Act.
        person.Delete();

        // Assert.
        IReadOnlyCollection<IDomainEvent> domainEvents = person.PullDomainEvents();
        domainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PersonDeletedEvent>()
            .Which.Person.Should().BeSameAs(person);
    }

    [Fact]
    public void AddPersonalAsset_WhenAssetIsNew_ShouldAddItToPersonalAssets()
    {
        // Arrange.
        Person person = PersonMother.ExistingPerson(Guid.NewGuid());
        PersonalAsset personalAsset = PersonMother.NewPersonalAsset();

        // Act.
        person.AddPersonalAsset(personalAsset);

        // Assert.
        person.PersonalAssets.Should().ContainSingle()
            .Which.Should().BeSameAs(personalAsset);
    }

    [Fact]
    public void AddPersonalAsset_WhenAssetIsNew_ShouldRegisterPersonalAssetAddedEvent()
    {
        // Arrange.
        Person person = PersonMother.ExistingPerson(Guid.NewGuid());
        PersonalAsset personalAsset = PersonMother.NewPersonalAsset();

        // Act.
        person.AddPersonalAsset(personalAsset);

        // Assert.
        IReadOnlyCollection<IDomainEvent> domainEvents = person.PullDomainEvents();
        PersonalAssetAddedEvent domainEvent = domainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PersonalAssetAddedEvent>().Subject;
        domainEvent.Person.Should().BeSameAs(person);
        domainEvent.PersonalAsset.Should().BeSameAs(personalAsset);
    }

    [Fact]
    public void LoadPersonalAsset_WhenAssetExists_ShouldAddItWithoutRegisteringDomainEvents()
    {
        // Arrange.
        Person person = PersonMother.ExistingPerson(Guid.NewGuid());
        PersonalAsset personalAsset = PersonMother.ExistingPersonalAsset(15);

        // Act.
        person.LoadPersonalAsset(personalAsset);

        // Assert.
        person.PersonalAssets.Should().ContainSingle()
            .Which.Should().BeSameAs(personalAsset);
        person.PullDomainEvents().Should().BeEmpty();
    }

    #endregion
}
