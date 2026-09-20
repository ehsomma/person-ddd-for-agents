using System.Diagnostics.CodeAnalysis;

namespace Records.Persons.Dtos.Person;

/// <summary>
/// Represents an address.
/// </summary>
[SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1629:DocumentationTextMustEndWithAPeriod",
    Justification = "Para permitir omitir el punto final en el tag <example> usado por OpenAPI.")]
public class Address
{
    #region Properties

    /// <summary>The id of the address.</summary>
    public int Id { get; init; }

    /// <summary>The street line 1.</summary>
    /// <example>4441 Collins Avenue</example>
    public string? StreetLine1 { get; init; }

    /// <summary>The street line 2.</summary>
    /// <example>null</example>
    public string? StreetLine2 { get; init; }

    /// <summary>The name of the city.</summary>
    /// <example>Miami Beach</example>
    public string? City { get; init; }

    /// <summary>The name of the state.</summary>
    /// <example>FL</example>
    public string? State { get; init; }

    /// <summary>The name of the country.</summary>
    /// <example>United States</example>
    public string? Country { get; init; }

    /// <summary>Represents the <see cref="Records.Persons.Dtos.Person.LatLng"/> (geographical point on Earth).</summary>
    public LatLng? LatLng { get; init; }

    #endregion
}
