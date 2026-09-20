using System.Diagnostics.CodeAnalysis;

namespace Records.Persons.Dtos.Person;

/// <summary>
/// Represents a personal asset.
/// </summary>
[SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1629:DocumentationTextMustEndWithAPeriod",
    Justification = "Para permitir omitir el punto final en el tag <example> usado por OpenAPI.")]
public class PersonalAsset
{
    #region Properties

    /// <summary>The PersonalAsset ID.</summary>
    /// <example>1</example>
    public int Id { get; init; } // Autoincrement.

    /// <summary>The description.</summary>
    /// <example>Oculus Quest 2</example>
    public string? Description { get; init; }

    /// <summary>The monetary value.</summary>
    /// <example>400</example>
    public decimal Value { get; init; }

    #endregion
}
