using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Application.Cqrs;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersonsWithSpecificProfile;

/// <summary>
/// Representa una query para obtener las Personas correspondientes a los filtros de género, edad mínima y ciudad.
/// </summary>
/// <remarks>
/// Se recibe directamente como el cuerpo del endpoint (POST), por lo que también documenta la solicitud en OpenAPI.
/// </remarks>
[SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1629:DocumentationTextMustEndWithAPeriod",
    Justification = "Para permitir omitir el punto final en el tag <example> usado por OpenAPI.")]
public sealed class GetPersonsWithSpecificProfileQuery : IQuery<IList<Dto.Person>>
{
    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="GetPersonsWithSpecificProfileQuery"/>.
    /// </summary>
    /// <param name="gender">El género de la Persona ("Male, Female, Other").</param>
    /// <param name="ageOlderThan">La edad mínima (inclusive).</param>
    /// <param name="liveInCity">El nombre de la ciudad donde vive la Persona.</param>
    public GetPersonsWithSpecificProfileQuery(string gender, int ageOlderThan, string liveInCity)
    {
        Gender = gender;
        AgeOlderThan = ageOlderThan;
        LiveInCity = liveInCity;
    }

    #endregion

    #region Properties

    /// <summary>El género de la Persona ("Male, Female, Other").</summary>
    /// <example>Male</example>
    public string Gender { get; } // Readonly.

    /// <summary>La edad mínima (inclusive).</summary>
    /// <example>30</example>
    public int AgeOlderThan { get; } // Readonly.

    /// <summary>El nombre de la ciudad donde vive la Persona.</summary>
    /// <example>Miami Beach</example>
    public string LiveInCity { get; } // Readonly.

    #endregion
}
