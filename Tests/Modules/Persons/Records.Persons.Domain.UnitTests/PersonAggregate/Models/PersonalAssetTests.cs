using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.ValueObjects;
using Records.Persons.Domain.PersonAggregate.Models;
using Records.Persons.Domain.PersonAggregate.ValueObjects;
using Records.Persons.TestData;
using Xunit;

namespace Records.Persons.Domain.UnitTests.PersonAggregate.Models;

/// <summary>
/// Tests de la entidad <see cref="PersonalAsset"/> (un bien personal de una persona).
/// </summary>
public sealed class PersonalAssetTests
{
    #region Public methods

    [Fact]
    public void Create_WhenDataIsValid_ShouldSetPropertiesAndHaveIdZero()
    {
        // Arrange.

        // Act.
        PersonalAsset personalAsset = PersonMother.NewPersonalAsset("Notebook", 1500M);

        // Assert.
        // Id = 0 es lo que Person.Update usa para distinguir un bien nuevo de uno existente.
        personalAsset.Id.Should().Be(0);
        personalAsset.Description.Value.Should().Be("Notebook");
        personalAsset.Value.Value.Should().Be(1500M);
    }

    [Fact]
    public void Create_WhenDescriptionIsNull_ShouldThrowDomainValidationException()
    {
        // Arrange.

        // Act.
        Action act = () => PersonalAsset.Create(null, Money.Build(1500M));

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Create_WhenValueIsNull_ShouldThrowDomainValidationException()
    {
        // Arrange.

        // Act.
        Action act = () => PersonalAsset.Create(PersonalAssetDescription.Build("Notebook"), null);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Load_WhenIdIsSpecified_ShouldKeepId()
    {
        // Arrange.

        // Act.
        PersonalAsset personalAsset = PersonMother.ExistingPersonalAsset(15);

        // Assert.
        personalAsset.Id.Should().Be(15);
    }

    #endregion
}
