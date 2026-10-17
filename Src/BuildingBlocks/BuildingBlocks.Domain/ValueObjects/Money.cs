using BuildingBlocks.Domain.Models;

namespace BuildingBlocks.Domain.ValueObjects;

/// <summary>
/// Represents the Money value object.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "StyleCop.CSharp.OrderingRules",
    "SA1201:Elements should appear in the correct order",
    Justification = "Prefere to have the Build() inside the constructor region.")]
public sealed class Money : ValueObject
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> value object.
    /// </summary>
    /// <param name="amount">The amount of money.</param>
    private Money(decimal amount)
    {
        Value = amount;
    }

    /// <summary>
    /// Build a new <see cref="Money"/> instance based on the specified <paramref name="amount"/>.
    /// </summary>
    /// <param name="amount">The amount of money.</param>
    /// <returns>The value object.</returns>
    public static Money Build(decimal amount)
    {
        Money money = new Money(amount);

        return money;
    }

    #endregion

    #region Properties

    /// <summary>Gets the value.</summary>
    public decimal Value { get; }

    #endregion

    #region Public methods

    /// <summary>
    /// Implicit operator that returns its value.
    /// </summary>
    /// <param name="money">The value object.</param>
    public static implicit operator decimal(Money money)
    {
        ArgumentNullException.ThrowIfNull(money);

        return money.Value;
    }

    /// <summary>
    /// Rounds the current value to the specified number of decimal places.
    /// </summary>
    /// <param name="decimals">Amount of decimals places (between 0 and 28 inclusive).</param>
    /// <returns>A new value object with the rounded value.</returns>
    public Money Round(int decimals)
    {
        Money rounded = Build(Math.Round(Value, decimals));

        return rounded;
    }

    /// <summary>
    /// Returns the value of the current <see cref="Money"/> as a <see cref="decimal"/>.
    /// </summary>
    /// <returns>The value.</returns>
    public decimal ToDecimal()
    {
        return Value;
    }

    #endregion

    #region Protected methods

    /// <inheritdoc />
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    #endregion
}
