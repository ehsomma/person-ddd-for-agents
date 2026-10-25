using System.Globalization;
using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using Records.Persons.Domain.PersonAggregate.ValueObjects;
using Xunit;

namespace Records.Persons.Domain.UnitTests.PersonAggregate.ValueObjects;

/// <summary>
/// Tests del value object <see cref="PersonalAssetValue"/>: de 0 a 999.999.999,99, con hasta 2
/// decimales.
/// </summary>
public sealed class PersonalAssetValueTests
{
    #region Public methods

    [Theory]
    [InlineData("0")] // Un bien sin valor declarado es válido.
    [InlineData("0.01")]
    [InlineData("1500.50")]
    [InlineData("999999999.99")] // El máximo.
    public void Build_WhenValueIsInRange_ShouldKeepValue(string value)
    {
        // Arrange.
        // NOTE: Los límites tienen que coincidir con la columna PersonalAssets.Value, decimal(11, 2)
        // (RecordsPersons_Source.sql), y con el PersonalAssetValidator de la API. Los valores llegan
        // como string porque InlineData no acepta decimal.
        decimal amount = decimal.Parse(value, CultureInfo.InvariantCulture);

        // Act.
        PersonalAssetValue personalAssetValue = PersonalAssetValue.Build(amount);

        // Assert.
        personalAssetValue.Value.Should().Be(amount);
    }

    [Fact]
    public void Build_WhenValueIsNegative_ShouldThrowDomainValidationException()
    {
        // Arrange.

        // Act.
        Action act = () => PersonalAssetValue.Build(-0.01M);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Build_WhenValueExceedsMaxValue_ShouldThrowDomainValidationException()
    {
        // Arrange.

        // Act.
        Action act = () => PersonalAssetValue.Build(PersonalAssetValue.MaxValue + 0.01M);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Theory]
    [InlineData("10.555")]
    [InlineData("0.001")]
    public void Build_WhenValueHasMoreThanTwoDecimals_ShouldThrowDomainValidationException(string value)
    {
        // Arrange.
        // Se rechaza en lugar de redondear: si se redondeara (o lo redondeara SQL Server al guardar),
        // el valor guardado no sería el que se mandó y nadie se enteraría.
        decimal amount = decimal.Parse(value, CultureInfo.InvariantCulture);

        // Act.
        Action act = () => PersonalAssetValue.Build(amount);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Build_WhenValueHasTrailingZeros_ShouldKeepValue()
    {
        // Arrange.
        // 10.500 tiene 3 decimales escritos, pero vale lo mismo que 10.50: los ceros finales no
        // cuentan como decimales de más.
        decimal amount = 10.500M;

        // Act.
        PersonalAssetValue personalAssetValue = PersonalAssetValue.Build(amount);

        // Assert.
        personalAssetValue.Value.Should().Be(10.5M);
    }

    [Fact]
    public void Equals_WhenValuesAreEqual_ShouldBeEqual()
    {
        // Arrange.
        PersonalAssetValue personalAssetValue = PersonalAssetValue.Build(1500.50M);
        PersonalAssetValue otherPersonalAssetValue = PersonalAssetValue.Build(1500.5M);

        // Act.
        bool areEqual = personalAssetValue == otherPersonalAssetValue;

        // Assert.
        // Se compara por valor: 1500.50 y 1500.5 son el mismo monto.
        areEqual.Should().BeTrue();
    }

    #endregion
}
