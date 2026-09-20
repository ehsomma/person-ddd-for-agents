using System.Diagnostics.CodeAnalysis;

namespace Records.Persons.Dtos.Person;

/// <summary>
/// Represents the <see cref="LatLng"/> (geographical point on Earth).
/// </summary>
[SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1629:DocumentationTextMustEndWithAPeriod",
    Justification = "Para permitir omitir el punto final en el tag <example> usado por OpenAPI.")]
public class LatLng
{
    #region Properties

    /// <summary>
    /// Latitude of the geographical point (angular distance from a point on the Earth's surface to
    /// the parallel of the equator). In WGS-84 format (e.g. 25.796549. "-90" to "90").
    /// </summary>
    /// <example>25.817887</example>
    public decimal? Lat { get; init; }

    /// <summary>
    /// Geographic point longitude (angular distance from a point on the Earth's surface to the
    /// Greenwich meridian). In WGS-84 format (e.g. -80.275613. "-180", "180").
    /// </summary>
    /// <example>-80.122785</example>
    public decimal? Lng { get; init; }

    #endregion
}
