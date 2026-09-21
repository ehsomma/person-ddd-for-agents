using System.Diagnostics.CodeAnalysis;

namespace Records.Persons.Dtos.Person;

/// <summary>
/// Represents a Person.
/// </summary>
[SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1629:DocumentationTextMustEndWithAPeriod",
    Justification = "Para permitir omitir el punto final en el tag <example> usado por OpenAPI.")]
public class Person
{
    #region Properties

    /// <summary>The Person ID.</summary>
    public Guid Id { get; init; }

    /// <summary>The full name.</summary>
    /// <example>Doe, John</example>
    public string? FullName { get; init; }

    /// <summary>The email.</summary>
    /// <example>johndoe@gmail.com</example>
    public string? Email { get; init; }

    /// <summary>The phone number.</summary>
    /// <example>1150011234</example>
    public string? Phone { get; init; }

    /// <summary>The gender ("Male, Female, Other").</summary>
    /// <example>Male</example>
    // TODO: Implement Gender enum.
    public string? Gender { get; init; }

    /// <summary>The birthdate.</summary>
    public DateTime? Birthdate { get; init; }

    /// <summary>The <see cref="Records.Persons.Dtos.Person.Address"/> of the person.</summary>
    public Address? Address { get; init; }

    /// <summary>The <see cref="PersonalAsset"/> list.</summary>
    public IList<PersonalAsset>? PersonalAssets { get; init; }

    #endregion
}
