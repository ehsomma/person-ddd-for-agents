using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.TestData;

/// <summary>
/// Crea los DTOs de personas (<see cref="Dto.Person"/>) con datos válidos para los tests (patrón
/// object mother).
/// <para>
/// Son los datos tal como llegan en el body de un request: la dirección y los bienes personales nuevos
/// vienen sin ID (0). Cada test parte de estos DTOs y cambia solo el dato que le interesa.
/// </para>
/// </summary>
public static class PersonDtoMother
{
    #region Public methods

    /// <summary>
    /// Crea el DTO de una persona con todos los datos válidos, una dirección con coordenadas y los
    /// bienes personales especificados.
    /// </summary>
    /// <param name="id">El ID de la persona.</param>
    /// <param name="personalAssets">Los bienes personales, o <see langword="null"/> si no tiene.</param>
    /// <returns>El DTO de la persona.</returns>
    public static Dto.Person ValidPerson(Guid id, IList<Dto.PersonalAsset>? personalAssets = null)
    {
        Dto.Person person = new Dto.Person
        {
            Id = id,
            FullName = "Roe, Jane",
            Email = "janeroe@gmail.com",
            Phone = "+1 305 555-0100",
            Gender = "Female",
            Birthdate = new DateTime(1990, 11, 25, 0, 0, 0, DateTimeKind.Utc),
            Address = new Dto.Address
            {
                StreetLine1 = "4441 Collins Avenue",
                StreetLine2 = "Apt 12B",
                City = "Miami Beach",
                State = "FL",
                Country = "United States",
                LatLng = new Dto.LatLng { Lat = 25.817887M, Lng = -80.122785M },
            },
            PersonalAssets = personalAssets,
        };

        return person;
    }

    /// <summary>
    /// Crea el DTO de un bien personal.
    /// </summary>
    /// <param name="id">El ID del bien (0 si es nuevo).</param>
    /// <param name="description">La descripción del bien.</param>
    /// <param name="value">El valor del bien.</param>
    /// <returns>El DTO del bien personal.</returns>
    public static Dto.PersonalAsset PersonalAsset(int id = 0, string description = "Notebook", decimal value = 1500M)
    {
        Dto.PersonalAsset personalAsset = new Dto.PersonalAsset
        {
            Id = id,
            Description = description,
            Value = value,
        };

        return personalAsset;
    }

    #endregion
}
