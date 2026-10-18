using System.Diagnostics.CodeAnalysis;

namespace Records.Persons.Dtos.Person;

/// <summary>
/// Representa la dirección de una persona.
/// </summary>
[SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1629:DocumentationTextMustEndWithAPeriod",
    Justification = "Para permitir omitir el punto final en el tag <example> usado por OpenAPI.")]
public class Address
{
    #region Properties

    /// <summary>ID de la dirección.</summary>
    public int Id { get; init; }

    /// <summary>Calle y número.</summary>
    /// <example>4441 Collins Avenue</example>
    public string? StreetLine1 { get; init; }

    /// <summary>Datos adicionales de la dirección, como piso o departamento (opcional).</summary>
    /// <example>null</example>
    public string? StreetLine2 { get; init; }

    /// <summary>Nombre de la ciudad.</summary>
    /// <example>Miami Beach</example>
    public string? City { get; init; }

    /// <summary>Nombre del estado o la provincia.</summary>
    /// <example>FL</example>
    public string? State { get; init; }

    /// <summary>Nombre del país.</summary>
    /// <example>United States</example>
    public string? Country { get; init; }

    /// <summary>Coordenadas geográficas de la dirección (<see cref="LatLng"/>), opcionales.</summary>
    public LatLng? LatLng { get; set; }

    #endregion
}
