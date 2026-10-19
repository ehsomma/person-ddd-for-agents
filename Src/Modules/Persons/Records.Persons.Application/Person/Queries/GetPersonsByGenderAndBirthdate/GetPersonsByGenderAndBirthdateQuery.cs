using BuildingBlocks.Application.Cqrs;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersonsByGenderAndBirthdate;

/// <summary>
/// Representa una query para obtener las Personas correspondientes a los filtros de género y período
/// de fecha de nacimiento.
/// </summary>
public sealed class GetPersonsByGenderAndBirthdateQuery : IQuery<IList<Dto.Person>>
{
    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="GetPersonsByGenderAndBirthdateQuery"/>.
    /// </summary>
    /// <param name="gender">El género de la Persona ("Male, Female, Other").</param>
    /// <param name="birthdateFrom">La fecha de nacimiento desde (inclusive).</param>
    /// <param name="birthdateTo">La fecha de nacimiento hasta (inclusive).</param>
    public GetPersonsByGenderAndBirthdateQuery(string gender, DateTime birthdateFrom, DateTime birthdateTo)
    {
        Gender = gender;
        BirthdateFrom = birthdateFrom;
        BirthdateTo = birthdateTo;
    }

    #endregion

    #region Properties

    /// <summary>El género de la Persona ("Male, Female, Other").</summary>
    public string Gender { get; } // Readonly.

    /// <summary>La fecha de nacimiento desde (inclusive).</summary>
    public DateTime BirthdateFrom { get; } // Readonly.

    /// <summary>La fecha de nacimiento hasta (inclusive).</summary>
    public DateTime BirthdateTo { get; } // Readonly.

    #endregion
}
