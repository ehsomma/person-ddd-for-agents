using AwesomeAssertions;
using BuildingBlocks.Domain.ValueObjects;
using Records.Persons.Application.Person.Mappers;
using Records.Persons.Domain.PersonAggregate.Enumerators;
using Records.Persons.Domain.PersonAggregate.ValueObjects;
using Records.Persons.TestData;
using Xunit;
using DomainModel = Records.Persons.Domain.PersonAggregate.Models; // Using aliases.
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.UnitTests.Person.Mappers;

/// <summary>
/// Tests del mapper <see cref="PersonMapper"/> de Application: del aggregate (dominio) al DTO que
/// devuelve la API.
/// </summary>
public sealed class PersonMapperTests
{
    #region Public methods

    [Fact]
    public void FromDomainToDto_WhenCalled_ShouldMapPersonData()
    {
        // Arrange.
        Guid id = Guid.NewGuid();
        DomainModel.Person domainPerson = PersonMother.ExistingPerson(id);
        PersonMapper mapper = new PersonMapper();

        // Act.
        Dto.Person personDto = mapper.FromDomainToDto(domainPerson);

        // Assert.
        personDto.Id.Should().Be(id);
        personDto.FullName.Should().Be(domainPerson.FullName.Value);
        personDto.Email.Should().Be(domainPerson.Email.Value);
        personDto.Phone.Should().Be(domainPerson.Phone!.Value);
        personDto.Gender.Should().Be(Gender.Male.Name);
        personDto.Birthdate.Should().Be(domainPerson.Birthdate);
    }

    [Fact]
    public void FromDomainToDto_WhenCalled_ShouldMapAddress()
    {
        // Arrange.
        DomainModel.Person domainPerson = PersonMother.ExistingPerson(Guid.NewGuid(), addressId: 7);
        DomainModel.Address domainAddress = domainPerson.Address;
        PersonMapper mapper = new PersonMapper();

        // Act.
        Dto.Person personDto = mapper.FromDomainToDto(domainPerson);

        // Assert.
        Dto.Address addressDto = personDto.Address!;
        addressDto.Id.Should().Be(7);
        addressDto.StreetLine1.Should().Be(domainAddress.StreetLine1.Value);
        addressDto.StreetLine2.Should().Be(domainAddress.StreetLine2!.Value);
        addressDto.City.Should().Be(domainAddress.City!.Value);
        addressDto.State.Should().Be(domainAddress.State!.Value);
        addressDto.Country.Should().Be(domainAddress.Country!.Value);
        addressDto.LatLng!.Lat.Should().Be(domainAddress.LatLng!.Lat);
        addressDto.LatLng.Lng.Should().Be(domainAddress.LatLng.Lng);
    }

    [Fact]
    public void FromDomainToDto_WhenCalled_ShouldMapPersonalAssets()
    {
        // Arrange.
        DomainModel.Person domainPerson = PersonMother.ExistingPerson(
            Guid.NewGuid(),
            personalAssets: new List<DomainModel.PersonalAsset>
            {
                PersonMother.ExistingPersonalAsset(15, "Bicicleta", 800M),
                PersonMother.ExistingPersonalAsset(16, "Notebook", 1500.50M),
            });
        PersonMapper mapper = new PersonMapper();

        // Act.
        Dto.Person personDto = mapper.FromDomainToDto(domainPerson);

        // Assert.
        personDto.PersonalAssets.Should().BeEquivalentTo(
            new Dto.PersonalAsset[]
            {
                new Dto.PersonalAsset { Id = 15, Description = "Bicicleta", Value = 800M },
                new Dto.PersonalAsset { Id = 16, Description = "Notebook", Value = 1500.50M },
            },
            options => options.WithStrictOrdering());
    }

    [Fact]
    public void FromDomainToDto_WhenOptionalDataIsMissing_ShouldMapNulls()
    {
        // Arrange.
        // Solo los datos obligatorios: sin teléfono, sin coordenadas y con la dirección mínima (solo
        // la primera línea). Los opcionales tienen que llegar al DTO como null, sin romper.
        DomainModel.Address domainAddress = DomainModel.Address.Create(
            StreetLine.Build("Av. Corrientes 1234"),
            null,
            null,
            null,
            null,
            null);
        DomainModel.Person domainPerson = DomainModel.Person.Create(
            Guid.NewGuid(),
            domainAddress,
            FullName.Build("Doe, John"),
            Email.Build("johndoe@gmail.com"),
            null,
            Gender.Male,
            null,
            null);
        PersonMapper mapper = new PersonMapper();

        // Act.
        Dto.Person personDto = mapper.FromDomainToDto(domainPerson);

        // Assert.
        personDto.Phone.Should().BeNull();
        personDto.Birthdate.Should().BeNull();
        personDto.Address!.StreetLine2.Should().BeNull();
        personDto.Address.City.Should().BeNull();
        personDto.Address.State.Should().BeNull();
        personDto.Address.Country.Should().BeNull();
        personDto.Address.LatLng.Should().BeNull();
        personDto.PersonalAssets.Should().BeEmpty();
    }

    [Fact]
    public void FromDomainToDto_WhenDomainPersonIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange.
        PersonMapper mapper = new PersonMapper();

        // Act.
        Action act = () => mapper.FromDomainToDto(null!);

        // Assert.
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}
