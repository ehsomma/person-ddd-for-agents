using BuildingBlocks.Application.Cqrs;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersonsByGenderAndBirthdate;

/// <summary>
/// Represents a query to get the Persons corresponding to the gender and birthdate period filters.
/// </summary>
public sealed class GetPersonsByGenderAndBirthdateQuery : IQuery<IList<Dto.Person>>
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPersonsByGenderAndBirthdateQuery"/> class.
    /// </summary>
    /// <param name="gender">The gender of the Person ("Male, Female, Other").</param>
    /// <param name="birthdateFrom">The birthdate from (inclusive).</param>
    /// <param name="birthdateTo">The birthdate to (inclusive).</param>
    public GetPersonsByGenderAndBirthdateQuery(string gender, DateTime birthdateFrom, DateTime birthdateTo)
    {
        Gender = gender;
        BirthdateFrom = birthdateFrom;
        BirthdateTo = birthdateTo;
    }

    #endregion

    #region Properties

    /// <summary>The gender of the Person ("Male, Female, Other").</summary>
    public string Gender { get; } // Readonly.

    /// <summary>The birthdate from (inclusive).</summary>
    public DateTime BirthdateFrom { get; } // Readonly.

    /// <summary>The birthdate to (inclusive).</summary>
    public DateTime BirthdateTo { get; } // Readonly.

    #endregion
}
