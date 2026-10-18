using System.Diagnostics.CodeAnalysis;

namespace Records.Persons.Dtos.Person;

/// <summary>
/// Representa un punto geográfico de la Tierra, con su latitud y su longitud en formato WGS-84.
/// </summary>
[SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1629:DocumentationTextMustEndWithAPeriod",
    Justification = "Para permitir omitir el punto final en el tag <example> usado por OpenAPI.")]
public class LatLng
{
    #region Properties

    /// <summary>Latitud del punto (distancia angular al ecuador), en grados decimales WGS-84, entre -90 y 90.</summary>
    /// <example>25.817887</example>
    public decimal? Lat { get; init; }

    /// <summary>Longitud del punto (distancia angular al meridiano de Greenwich), en grados decimales WGS-84, entre -180 y 180.</summary>
    /// <example>-80.122785</example>
    public decimal? Lng { get; init; }

    #endregion
}
