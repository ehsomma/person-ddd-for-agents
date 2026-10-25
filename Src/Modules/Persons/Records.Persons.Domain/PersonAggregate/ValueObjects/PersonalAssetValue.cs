using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.Models;
using Throw;

namespace Records.Persons.Domain.PersonAggregate.ValueObjects;

/// <summary>
/// Representa el valor monetario de un bien personal: de 0 a 999.999.999,99, con hasta 2 decimales.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "StyleCop.CSharp.OrderingRules",
    "SA1201:Elements should appear in the correct order",
    Justification = "Build() va dentro de la region Constructor.")]
public sealed class PersonalAssetValue : ValueObject
{
    #region Declarations

    /// <summary>El valor máximo: el que entra en la columna PersonalAssets.Value, decimal(11, 2).</summary>
    public const decimal MaxValue = 999_999_999.99M;

    /// <summary>La cantidad máxima de decimales (la escala de la columna).</summary>
    public const int MaxDecimals = 2;

    #endregion

    #region Constructor

    /// <summary>Inicializa una nueva instancia de <see cref="PersonalAssetValue"/>.</summary>
    /// <param name="value">El valor.</param>
    private PersonalAssetValue(decimal value)
    {
        EnsureIsValid(value);

        Value = value;
    }

    /// <summary>
    /// Crea el valor de un bien personal y valida que esté entre 0 y <see cref="MaxValue"/> y que no
    /// tenga más de <see cref="MaxDecimals"/> decimales.
    /// </summary>
    /// <param name="value">El valor.</param>
    /// <returns>El value object.</returns>
    /// <exception cref="DomainValidationException">Si el valor no es válido.</exception>
    public static PersonalAssetValue Build(decimal value)
    {
        PersonalAssetValue personalAssetValue = new PersonalAssetValue(value);

        return personalAssetValue;
    }

    #endregion

    #region Properties

    /// <summary>El valor.</summary>
    public decimal Value { get; }

    #endregion

    #region Protected methods

    /// <inheritdoc />
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Valida que el valor esté entre 0 y <see cref="MaxValue"/> y que no tenga más de
    /// <see cref="MaxDecimals"/> decimales.
    /// </summary>
    /// <param name="value">El valor.</param>
    /// <exception cref="DomainValidationException">Si el valor no es válido.</exception>
    private static void EnsureIsValid(decimal value)
    {
        try
        {
            value.Throw().IfLessThan(0).IfGreaterThan(MaxValue);
        }
        catch (Exception ex)
        {
            throw new DomainValidationException(ex.Message, ex);
        }

        // Con más decimales se rechaza, no se redondea: SQL Server redondearía al guardar y el valor
        // que se devuelve no sería el que se mandó, sin ningún error. Se compara con el valor
        // redondeado (y no con decimal.Scale) para que los ceros finales no cuenten:
        //   10.5    → válido ✓
        //   10.500  → válido ✓ (Scale = 3, pero vale lo mismo que 10.50)
        //   10.555  → inválido ✗
        if (decimal.Round(value, MaxDecimals) != value)
        {
            throw new DomainValidationException($"PersonalAssetValue must have at most {MaxDecimals} decimals.");
        }
    }

    #endregion
}
