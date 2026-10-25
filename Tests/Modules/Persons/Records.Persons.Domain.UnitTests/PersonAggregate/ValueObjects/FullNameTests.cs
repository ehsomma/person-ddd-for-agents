using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using Records.Persons.Domain.PersonAggregate.ValueObjects;
using Xunit;

namespace Records.Persons.Domain.UnitTests.PersonAggregate.ValueObjects;

/// <summary>
/// Tests del value object <see cref="FullName"/>: obligatorio, de 1 a 62 caracteres.
/// </summary>
public sealed class FullNameTests
{
    #region Declarations

    // NOTE: Tiene que coincidir con el largo de la columna en la base (RecordsPersons_Source.sql).
    // Si este test falla porque cambió el límite, revisá también la columna.
    private const int MaxLength = 62;

    #endregion

    #region Public methods

    [Fact]
    public void Build_WhenValueIsValid_ShouldKeepValue()
    {
        // Arrange.

        // Act.
        FullName fullName = FullName.Build("Doe, John");

        // Assert.
        fullName.Value.Should().Be("Doe, John");
    }

    [Fact]
    public void Build_WhenValueHasMaxLength_ShouldKeepValue()
    {
        // Arrange.
        string value = new string('a', MaxLength);

        // Act.
        FullName fullName = FullName.Build(value);

        // Assert.
        fullName.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Build_WhenValueIsNullOrEmpty_ShouldThrowDomainValidationException(string? value)
    {
        // Arrange.

        // Act.
        Action act = () => FullName.Build(value);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Build_WhenValueExceedsMaxLength_ShouldThrowDomainValidationException()
    {
        // Arrange.
        string value = new string('a', MaxLength + 1);

        // Act.
        Action act = () => FullName.Build(value);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Equals_WhenValuesAreEqual_ShouldBeEqual()
    {
        // Arrange.
        FullName fullName = FullName.Build("Doe, John");
        FullName otherFullName = FullName.Build("Doe, John");

        // Act.
        bool areEqual = fullName == otherFullName;

        // Assert.
        // Un value object se compara por su valor, no por la referencia.
        areEqual.Should().BeTrue();
        fullName.Should().Be(otherFullName);
    }

    #endregion
}
