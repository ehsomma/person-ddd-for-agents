using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using Records.Persons.Domain.PersonAggregate.ValueObjects;
using Xunit;

namespace Records.Persons.Domain.UnitTests.PersonAggregate.ValueObjects;

/// <summary>
/// Tests del value object <see cref="StreetLine"/>: obligatorio, de 1 a 60 caracteres.
/// </summary>
public sealed class StreetLineTests
{
    #region Declarations

    // NOTE: Tiene que coincidir con el largo de la columna en la base (RecordsPersons_Source.sql).
    // Si este test falla porque cambió el límite, revisá también la columna.
    private const int MaxLength = 60;

    #endregion

    #region Public methods

    [Fact]
    public void Build_WhenValueHasMaxLength_ShouldKeepValue()
    {
        // Arrange.
        string value = new string('a', MaxLength);

        // Act.
        StreetLine streetLine = StreetLine.Build(value);

        // Assert.
        streetLine.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Build_WhenValueIsNullOrEmpty_ShouldThrowDomainValidationException(string? value)
    {
        // Arrange.

        // Act.
        Action act = () => StreetLine.Build(value);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Build_WhenValueExceedsMaxLength_ShouldThrowDomainValidationException()
    {
        // Arrange.
        string value = new string('a', MaxLength + 1);

        // Act.
        Action act = () => StreetLine.Build(value);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    #endregion
}
