using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using Records.Persons.Domain.Shared.ValueObjects;
using Xunit;

namespace Records.Persons.Domain.UnitTests.Shared.ValueObjects;

/// <summary>
/// Tests del value object <see cref="CountryName"/>: opcional, acepta <see langword="null"/> y vacío,
/// hasta 160 caracteres.
/// </summary>
public sealed class CountryNameTests
{
    #region Declarations

    // TODO: El value object acepta hasta 160 caracteres, pero la columna Addresses.Country es
    // nvarchar(60) (RecordsPersons_Source.sql): un país de 61 a 160 caracteres pasa el dominio y falla
    // al insertar. Hay que decidir cuál es el límite correcto y alinear el value object, la columna y
    // este test.
    private const int MaxLength = 160;

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
