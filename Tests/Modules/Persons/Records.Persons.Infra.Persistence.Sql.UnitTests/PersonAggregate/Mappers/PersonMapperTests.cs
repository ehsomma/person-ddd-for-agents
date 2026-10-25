using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.ValueObjects;
using Records.Persons.Domain.PersonAggregate.Enumerators;
using Records.Persons.Domain.PersonAggregate.ValueObjects;
using Records.Persons.Infra.Persistence.Sql.PersonAggregate.Mappers;
using Records.Persons.TestData;
using Xunit;
using DataModel = Records.Persons.Infra.Persistence.Sql.PersonAggregate.Models; // Using aliases.
using DomainModel = Records.Persons.Domain.PersonAggregate.Models; // Using aliases.

namespace Records.Persons.Infra.Persistence.Sql.UnitTests.PersonAggregate.Mappers;

/// <summary>
/// Tests del mapper <see cref="PersonMapper"/> de persistencia: del aggregate (dominio) a los data
/// models que se guardan con Dapper, y de vuelta.
/// </summary>
public sealed class PersonMapperTests
{
    #region Public methods

    [Fact]
    public void FromDomainToDataModel_WhenCalled_ShouldMapPersonDataAndAuditDates()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        DomainModel.Person domainPerson = PersonMother.ExistingPerson(id);
        PersonMapper mapper = new PersonMapper();

        // Act.
        DataModel.Person dataPerson = mapper.FromDomainToDataModel(domainPerson);

        // Assert.
        dataPerson.Id.Should().Be(id);
        dataPerson.FullName.Should().Be(domainPerson.FullName.Value);
        dataPerson.Email.Should().Be(domainPerson.Email.Value);
        dataPerson.Phone.Should().Be(domainPerson.Phone!.Value);
        dataPerson.Gender.Should().Be(domainPerson.Gender.Name); // Se guarda el nombre, no el valor.
        dataPerson.Birthdate.Should().Be(domainPerson.Birthdate);
        dataPerson.CreatedOnUtc.Should().Be(PersonMother.CreatedOnUtc);
        dataPerson.UpdatedOnUtc.Should().Be(PersonMother.UpdatedOnUtc);
    }

    [Fact]
    public void FromDomainToDataModel_WhenCalled_ShouldMapAddressWithPersonIdAsForeignKey()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        DomainModel.Person domainPerson = PersonMother.ExistingPerson(id, addressId: 7);
        DomainModel.Address domainAddress = domainPerson.Address;
        PersonMapper mapper = new PersonMapper();

        // Act.
        DataModel.Person dataPerson = mapper.FromDomainToDataModel(domainPerson);

        // Assert.
        // La dirección no tiene el ID de la persona en el dominio (es parte del aggregate), pero en
        // la tabla es la FK: el mapper la completa.
        DataModel.Address dataAddress = dataPerson.Address!;
        dataAddress.Id.Should().Be(7);
        dataAddress.PersonId.Should().Be(id);
        dataAddress.StreetLine1.Should().Be(domainAddress.StreetLine1.Value);
        dataAddress.StreetLine2.Should().Be(domainAddress.StreetLine2!.Value);
        dataAddress.City.Should().Be(domainAddress.City!.Value);
        dataAddress.State.Should().Be(domainAddress.State!.Value);
        dataAddress.Country.Should().Be(domainAddress.Country!.Value);
        dataAddress.Lat.Should().Be(domainAddress.LatLng!.Lat);
        dataAddress.Lng.Should().Be(domainAddress.LatLng.Lng);
    }

    [Fact]
    public void FromDomainToDataModel_WhenCalled_ShouldMapPersonalAssetsWithPersonIdAsForeignKey()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        DomainModel.Person domainPerson = PersonMother.ExistingPerson(
            id,
            personalAssets: new List<DomainModel.PersonalAsset>
            {
                PersonMother.ExistingPersonalAsset(15, "Bicicleta", 800M),
                PersonMother.ExistingPersonalAsset(16, "Notebook", 1500.50M),
            });
        PersonMapper mapper = new PersonMapper();

        // Act.
        DataModel.Person dataPerson = mapper.FromDomainToDataModel(domainPerson);

        // Assert.
        dataPerson.PersonalAssets.Should().BeEquivalentTo(
            new DataModel.PersonalAsset[]
            {
                new DataModel.PersonalAsset { Id = 15, PersonId = id, Description = "Bicicleta", Value = 800M },
                new DataModel.PersonalAsset { Id = 16, PersonId = id, Description = "Notebook", Value = 1500.50M },
            },
            options => options.WithStrictOrdering());
    }

    [Fact]
    public void FromDomainToDataModel_WhenAddressHasNoCoordinates_ShouldMapLatAndLngAsNull()
    {
        // Arrange.
        // Las dos columnas van juntas: la tabla tiene un CHECK que exige las dos en null o las dos
        // con valor (CK_Addresses_LatLng).
        DomainModel.Person domainPerson = DomainModel.Person.Load(
            Guid.NewGuid(),
            DomainModel.Address.Load(7, StreetLine.Build("Av. Corrientes 1234"), null, null, null, null, null),
            FullName.Build("Doe, John"),
            Email.Build("johndoe@gmail.com"),
            null,
            Gender.Male,
            null,
            null);
        PersonMapper mapper = new PersonMapper();

        // Act.
        DataModel.Person dataPerson = mapper.FromDomainToDataModel(domainPerson);

        // Assert.
        dataPerson.Phone.Should().BeNull();
        dataPerson.Address!.Lat.Should().BeNull();
        dataPerson.Address.Lng.Should().BeNull();
    }

    [Fact]
    public void FromDataModelToDomain_WhenMappedBack_ShouldRebuildTheSameAggregate()
    {
        // Arrange.
        // Ida y vuelta (dominio → data model → dominio): si un campo nuevo se mapea en una sola
        // dirección, el aggregate que vuelve es distinto al original y este test falla.
        DomainModel.Person domainPerson = PersonMother.ExistingPerson(
            Guid.NewGuid(),
            personalAssets: new List<DomainModel.PersonalAsset> { PersonMother.ExistingPersonalAsset(15) });
        PersonMapper mapper = new PersonMapper();
        DataModel.Person dataPerson = mapper.FromDomainToDataModel(domainPerson);

        // Act.
        DomainModel.Person? mappedBackPerson = mapper.FromDataModelToDomain(dataPerson);

        // Assert.
        // ComparingByMembers: las entidades redefinen Equals por ID, así que sin esto
        // BeEquivalentTo compararía solo los IDs y el test pasaría aunque faltara un campo.
        mappedBackPerson.Should().BeEquivalentTo(
            domainPerson,
            options => options
                .ComparingByMembers<DomainModel.Person>()
                .ComparingByMembers<DomainModel.Address>()
                .ComparingByMembers<DomainModel.PersonalAsset>());
    }

    [Fact]
    public void FromDataModelToDomain_WhenCalled_ShouldNotRegisterDomainEvents()
    {
        // Arrange.
        // Es una persona que ya existe en la base: se carga con Load y LoadPersonalAsset, que no
        // registran eventos. Si registrara alguno, el command lo publicaría como si fuera nuevo.
        PersonMapper mapper = new PersonMapper();
        DataModel.Person dataPerson = mapper.FromDomainToDataModel(
            PersonMother.ExistingPerson(
                Guid.NewGuid(),
                personalAssets: new List<DomainModel.PersonalAsset> { PersonMother.ExistingPersonalAsset(15) }));

        // Act.
        DomainModel.Person? domainPerson = mapper.FromDataModelToDomain(dataPerson);

        // Assert.
        domainPerson!.PullDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void FromDataModelToDomain_WhenDataPersonIsNull_ShouldReturnNull()
    {
        // Arrange.
        // El repositorio devuelve null cuando la persona no existe, y es el servicio de dominio el que
        // lo convierte en DomainErrors.Person.NotFound: el mapper no tiene que lanzar nada.
        PersonMapper mapper = new PersonMapper();

        // Act.
        DomainModel.Person? domainPerson = mapper.FromDataModelToDomain(null);

        // Assert.
        domainPerson.Should().BeNull();
    }

    [Theory]
    [InlineData(-34.603722, null)]
    [InlineData(null, -58.381592)]
    public void FromDataModelToDomain_WhenOnlyOneCoordinateHasValue_ShouldThrowDomainValidationException(double? lat, double? lng)
    {
        // Arrange.
        // Una fila con una sola coordenada es un dato corrupto: no se descarta en silencio (ver el
        // comentario del "||" en el mapper).
        DataModel.Person dataPerson = new DataModel.Person
        {
            Id = Guid.NewGuid(),
            FullName = "Doe, John",
            Email = "johndoe@gmail.com",
            Gender = "Male",
            Address = new DataModel.Address
            {
                Id = 7,
                StreetLine1 = "Av. Corrientes 1234",
                Lat = (decimal?)lat,
                Lng = (decimal?)lng,
            },
        };
        PersonMapper mapper = new PersonMapper();

        // Act.
        Action act = () => mapper.FromDataModelToDomain(dataPerson);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void FromDataModelToDomain_WhenAddressIsMissing_ShouldThrowDomainValidationException()
    {
        // Arrange.
        // La dirección es obligatoria en el aggregate: una persona sin fila en Addresses no se puede
        // cargar. El mapper le pasa la dirección en null a Person.Load, y es la invariante del
        // aggregate la que lanza la excepción.
        DataModel.Person dataPerson = new DataModel.Person
        {
            Id = Guid.NewGuid(),
            FullName = "Doe, John",
            Email = "johndoe@gmail.com",
            Gender = "Male",
            Address = null,
        };
        PersonMapper mapper = new PersonMapper();

        // Act.
        Action act = () => mapper.FromDataModelToDomain(dataPerson);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    #endregion
}
