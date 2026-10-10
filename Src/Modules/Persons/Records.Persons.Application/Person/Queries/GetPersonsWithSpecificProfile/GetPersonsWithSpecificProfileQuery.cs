using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Application.Cqrs;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersonsWithSpecificProfile;

/// <summary>
/// Represents a query to get the Persons corresponding to the gender, minimum age and city filters.
/// </summary>
/// <remarks>
/// It's received directly as the body of the endpoint (POST), so it also documents the request in OpenAPI.
/// </remarks>
[SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1629:DocumentationTextMustEndWithAPeriod",
    Justification = "Para permitir omitir el punto final en el tag <example> usado por OpenAPI.")]
public sealed class GetPersonsWithSpecificProfileQuery : IQuery<IList<Dto.Person>>
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPersonsWithSpecificProfileQuery"/> class.
    /// </summary>
    /// <param name="gender">The gender of the Person ("Male, Female, Other").</param>
    /// <param name="ageOlderThan">The minimum age (inclusive).</param>
    /// <param name="liveInCity">The name of the city where the Person lives.</param>
    public GetPersonsWithSpecificProfileQuery(string gender, int ageOlderThan, string liveInCity)
    {
        Gender = gender;
        AgeOlderThan = ageOlderThan;
        LiveInCity = liveInCity;
    }

    #endregion

    #region Properties

    /// <summary>The gender of the Person ("Male, Female, Other").</summary>
    /// <example>Male</example>
    public string Gender { get; } // Readonly.

    /// <summary>The minimum age (inclusive).</summary>
    /// <example>30</example>
    public int AgeOlderThan { get; } // Readonly.

    /// <summary>The name of the city where the Person lives.</summary>
    /// <example>Miami Beach</example>
    public string LiveInCity { get; } // Readonly.

    #endregion
}
