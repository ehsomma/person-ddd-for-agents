using System.Diagnostics.CodeAnalysis;

namespace Records.Persons.Dtos.Person;

/// <summary>
/// Representa un bien personal de una persona.
/// </summary>
[SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1629:DocumentationTextMustEndWithAPeriod",
    Justification = "Para permitir omitir el punto final en el tag <example> usado por OpenAPI.")]
public class PersonalAsset
{
    #region Properties

    /// <summary>ID del bien personal. En un bien personal nuevo se envía 0, porque lo genera el sistema.</summary>
    /// <example>1</example>
    public int Id { get; init; } // Autoincremental.

    /// <summary>Descripción del bien personal.</summary>
    /// <example>Oculus Quest 2</example>
    public string? Description { get; init; }

    /// <summary>Valor monetario del bien personal.</summary>
    /// <example>400</example>
    public decimal Value { get; init; }

    #endregion
}
