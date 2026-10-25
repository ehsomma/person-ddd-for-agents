using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using Records.Persons.Domain.PersonAggregate.Models;
using Records.Persons.Domain.PersonAggregate.ValueObjects;
using Records.Persons.TestData;
using Xunit;

namespace Records.Persons.Domain.UnitTests.PersonAggregate.Models;

/// <summary>
/// Tests de la entidad <see cref="Address"/> (la dirección de una persona).
/// </summary>
public sealed class AddressTests
{
    #region Public methods

    [Fact]
    public void Create_WhenDataIsValid_ShouldHaveIdZero()
    {
        // Arrange.

        // Act.
        Address address = PersonMother.NewAddress();

        // Assert.
        // Las entidades hijas nuevas tienen ID 0: el ID real lo genera la base al insertarla.
        address.Id.Should().Be(0);
    }

    [Fact]
    public void Create_WhenOnlyStreetLine1IsSpecified_ShouldCreateAddress()
    {
        // Arrange.

        // Act.
        Address address = Address.Create(StreetLine.Build("Av. Corrientes 1234"), null, null, null, null, null);

        // Assert.
        address.StreetLine1.Value.Should().Be("Av. Corrientes 1234");
        address.StreetLine2.Should().BeNull();
        address.City.Should().BeNull();
        address.State.Should().BeNull();
        address.Country.Should().BeNull();
        address.LatLng.Should().BeNull();
    }

    [Fact]
    public void Create_WhenStreetLine1IsNull_ShouldThrowDomainValidationException()
    {
        // Arrange.

        // Act.
        Action act = () => Address.Create(null, null, null, null, null, null);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Load_WhenIdIsSpecified_ShouldKeepId()
    {
        // Arrange.

        // Act.
        Address address = PersonMother.ExistingAddress(7);

        // Assert.
        address.Id.Should().Be(7);
    }

    #endregion
}
