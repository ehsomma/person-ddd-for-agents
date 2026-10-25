using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using Records.Persons.Domain.PersonAggregate.Enumerators;
using Xunit;

namespace Records.Persons.Domain.UnitTests.PersonAggregate.Enumerators;

/// <summary>
/// Tests de la enumeración <see cref="Gender"/>.
/// </summary>
public sealed class GenderTests
{
    #region Public methods

    [Theory]
    [InlineData("Male", 1)]
    [InlineData("Female", 2)]
    [InlineData("Other", 3)]
    public void FromName_WhenNameIsValid_ShouldReturnGender(string name, int expectedValue)
    {
        // Arrange.

        // Act.
        Gender gender = Gender.FromName(name);

        // Assert.
        gender.Value.Should().Be(expectedValue);
        gender.Name.Should().Be(name);
    }

    [Theory]
    [InlineData("male")]
    [InlineData("FEMALE")]
    [InlineData("oThEr")]
    public void FromName_WhenNameHasDifferentCase_ShouldReturnGender(string name)
    {
        // Arrange.

        // Act.
        Gender gender = Gender.FromName(name);

        // Assert.
        // La comparación no distingue mayúsculas: el nombre que queda es el de la enumeración.
        gender.Name.Should().BeEquivalentTo(name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("Martian")]
    public void FromName_WhenNameIsInvalid_ShouldThrowDomainValidationException(string? name)
    {
        // Arrange.

        // Act.
        Action act = () => Gender.FromName(name);

        // Assert.
        act.Should().Throw<DomainValidationException>()
            .WithMessage("Possible values for Gender: *");
    }

    [Theory]
    [InlineData(1, "Male")]
    [InlineData(2, "Female")]
    [InlineData(3, "Other")]
    public void FromValue_WhenValueIsValid_ShouldReturnGender(int value, string expectedName)
    {
        // Arrange.

        // Act.
        Gender gender = Gender.FromValue(value);

        // Assert.
        gender.Name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(-1)]
    public void FromValue_WhenValueIsInvalid_ShouldThrowDomainValidationException(int value)
    {
        // Arrange.

        // Act.
        Action act = () => Gender.FromValue(value);

        // Assert.
        act.Should().Throw<DomainValidationException>()
            .WithMessage("Possible values for Gender: *");
    }

    #endregion
}
