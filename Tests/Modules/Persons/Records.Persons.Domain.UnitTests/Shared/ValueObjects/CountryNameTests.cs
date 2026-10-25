using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using Records.Persons.Domain.Shared.ValueObjects;
using Xunit;

namespace Records.Persons.Domain.UnitTests.Shared.ValueObjects;

/// <summary>
/// Tests del value object <see cref="CountryName"/>: opcional, acepta <see langword="null"/> y vacío,
/// hasta 60 caracteres.
/// </summary>
public sealed class CountryNameTests
{
    #region Declarations

    // NOTE: Tiene que coincidir con el largo de la columna en la base (RecordsPersons_Source.sql).
    // Si este test falla porque cambió el límite, revisá también la columna. 60 alcanza para el nombre
    // de país más largo en uso ("The United Kingdom of Great Britain and Northern Ireland", 56).
    private const int MaxLength = 60;

    #endregion

    #region Public methods

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Build_WhenValueIsNullOrEmpty_ShouldHaveNoValue(string? value)
    {
        // Arrange.

        // Act.
        // A diferencia de City o State (mínimo 1), acá el mínimo es 0: el vacío también es válido.
        CountryName countryName = CountryName.Build(value);

        // Assert.
        countryName.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Build_WhenValueHasMaxLength_ShouldKeepValue()
    {
        // Arrange.
        string value = new string('a', MaxLength);

        // Act.
        CountryName countryName = CountryName.Build(value);

        // Assert.
        countryName.Value.Should().Be(value);
    }

    [Fact]
    public void Build_WhenValueExceedsMaxLength_ShouldThrowDomainValidationException()
    {
        // Arrange.
        string value = new string('a', MaxLength + 1);

        // Act.
        Action act = () => CountryName.Build(value);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    #endregion
}
