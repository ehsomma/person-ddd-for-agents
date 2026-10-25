using AwesomeAssertions;
using BuildingBlocks.Domain.ValueObjects;
using Xunit;

namespace BuildingBlocks.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests of the <see cref="Money"/> value object: any amount, rounding and conversion to
/// <see cref="decimal"/>.
/// </summary>
public sealed class MoneyTests
{
    #region Public methods

    [Theory]
    [InlineData(1500.50)]
    [InlineData(0)]
    [InlineData(-10)]
    public void Build_WhenCalled_ShouldKeepAmount(double amount)
    {
        // Arrange.
        // NOTE: Money no valida el monto (acepta negativos y cualquier largo): los límites son de
        // cada módulo. En Persons, PersonalAssets.Value es decimal(9, 2) y el único que valida es el
        // PersonalAssetValidator de la API, que solo exige que sea mayor a 0 (ver su TODO).
        decimal value = (decimal)amount;

        // Act.
        Money money = Money.Build(value);

        // Assert.
        money.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(10.554, 10.55)]
    [InlineData(10.556, 10.56)]
    [InlineData(10.125, 10.12)] // Punto medio: al par, no para arriba.
    [InlineData(10.135, 10.14)] // Punto medio: al par.
    public void Round_WhenDecimalsAreSpecified_ShouldRoundToEven(double amount, double expected)
    {
        // Arrange.
        // ⚠️ Math.Round sin MidpointRounding redondea los puntos medios al par (redondeo bancario):
        //   10.125 → 10.12 (no 10.13)
        //   10.135 → 10.14
        // Si el negocio necesita el redondeo "de la escuela" (MidpointRounding.AwayFromZero), hay
        // que cambiar Money.Round y estos casos.
        Money money = Money.Build((decimal)amount);

        // Act.
        Money rounded = money.Round(2);

        // Assert.
        rounded.Value.Should().Be((decimal)expected);
    }

    [Fact]
    public void Round_WhenCalled_ShouldNotChangeOriginal()
    {
        // Arrange.
        Money money = Money.Build(10.556M);

        // Act.
        Money rounded = money.Round(2);

        // Assert.
        // Un value object es inmutable: Round devuelve uno nuevo.
        rounded.Should().NotBeSameAs(money);
        money.Value.Should().Be(10.556M);
    }

    [Fact]
    public void ImplicitOperator_WhenCalled_ShouldReturnAmount()
    {
        // Arrange.
        Money money = Money.Build(1500.50M);

        // Act.
        decimal value = money;

        // Assert.
        value.Should().Be(1500.50M);
    }

    [Fact]
    public void Equals_WhenAmountsAreEqual_ShouldBeEqual()
    {
        // Arrange.
        Money money = Money.Build(1500.50M);
        Money otherMoney = Money.Build(1500.50M);

        // Act.
        bool areEqual = money == otherMoney;

        // Assert.
        areEqual.Should().BeTrue();
    }

    #endregion
}
