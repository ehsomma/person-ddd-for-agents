using BuildingBlocks.Domain.ValueObjects;

namespace Records.Persons.Domain.PersonAggregate.ValueObjects;

/// <summary>
/// Representa el value object del nombre completo de una persona.
/// </summary>
public sealed class FullName : StringValueObject
{
    #region Constructor

    /// <summary>Inicializa una nueva instancia de <see cref="FullName"/>.</summary>
    /// <param name="value">El nombre completo de la persona.</param>
    private FullName(string? value)
        : base(value, "FullName", 62, 1)
    {
    }

    /// <summary>
    /// Crea un <see cref="FullName"/> a partir de <paramref name="value"/>. La validación la hace
    /// <see cref="StringValueObject"/>: no puede ser null y tiene que tener entre 1 y 62
    /// caracteres.
    /// </summary>
    /// <param name="value">El nombre completo de la persona.</param>
    /// <returns>El value object creado.</returns>
    public static FullName Build(string? value)
    {
        return new FullName(value);
    }

    #endregion
}
