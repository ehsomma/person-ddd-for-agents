using BuildingBlocks.Domain.ValueObjects;
using Records.Persons.Domain.PersonAggregate.Enumerators;
using Records.Persons.Domain.PersonAggregate.Models;
using Records.Persons.Domain.PersonAggregate.ValueObjects;
using Records.Persons.Domain.Shared.ValueObjects;

namespace Records.Persons.Domain.UnitTests.TestData;

/// <summary>
/// Crea los objetos del aggregate de personas (<see cref="Person"/>) con datos válidos para los tests
/// (patrón object mother).
/// <para>
/// Cada test parte de estos objetos y cambia solo el dato que le interesa, así el Arrange queda corto
/// y se ve de un vistazo qué es lo que distingue al escenario. Los métodos <c>New*</c> usan
/// <c>Create</c> (una persona nueva, con el evento de creación registrado) y los <c>Existing*</c>
/// usan <c>Load</c> (una persona ya guardada, tal como la devuelve el repositorio, sin eventos).
/// </para>
/// </summary>
internal static class PersonMother
{
    #region Declarations

    /// <summary>La fecha de alta de las personas existentes.</summary>
    public static readonly DateTime CreatedOnUtc = new DateTime(2020, 1, 15, 10, 30, 0, DateTimeKind.Utc);

    /// <summary>La fecha de la última modificación de las personas existentes.</summary>
    public static readonly DateTime UpdatedOnUtc = new DateTime(2021, 6, 20, 18, 45, 0, DateTimeKind.Utc);

    #endregion

    #region Public methods

    /// <summary>
    /// Crea una persona nueva (con <see cref="Person.Create"/>), con todos los datos válidos y sin
    /// bienes personales.
    /// </summary>
    /// <returns>La persona creada, con el <c>PersonCreatedEvent</c> registrado.</returns>
    public static Person NewPerson()
    {
        Person person = Person.Create(
            Guid.NewGuid(),
            NewAddress(),
            FullName.Build("Doe, John"),
            Email.Build("johndoe@gmail.com"),
            PhoneNumber.Build("+54 11 4444-5555"),
            Gender.Male,
            null,
            new DateTime(1985, 3, 10, 0, 0, 0, DateTimeKind.Utc));

        return person;
    }

    /// <summary>
    /// Carga una persona existente (con <see cref="Person.Load"/>), con la dirección
    /// <paramref name="addressId"/>, las fechas de auditoría <see cref="CreatedOnUtc"/> y
    /// <see cref="UpdatedOnUtc"/> y los bienes personales especificados.
    /// </summary>
    /// <param name="id">El ID de la persona.</param>
    /// <param name="addressId">El ID de la dirección.</param>
    /// <param name="personalAssets">Los bienes personales, o <see langword="null"/> si no tiene.</param>
    /// <returns>La persona cargada, sin eventos registrados.</returns>
    public static Person ExistingPerson(Guid id, int addressId = 7, IList<PersonalAsset>? personalAssets = null)
    {
        Person person = Person.Load(
            id,
            ExistingAddress(addressId),
            FullName.Build("Doe, John"),
            Email.Build("johndoe@gmail.com"),
            PhoneNumber.Build("+54 11 4444-5555"),
            Gender.Male,
            personalAssets,
            new DateTime(1985, 3, 10, 0, 0, 0, DateTimeKind.Utc),
            CreatedOnUtc,
            UpdatedOnUtc);

        return person;
    }

    /// <summary>
    /// Carga los datos editados de una persona, como los arma un command handler a partir del body
    /// del request: todos los datos distintos a los de <see cref="ExistingPerson"/>, con una dirección
    /// sin ID y con los bienes personales especificados.
    /// </summary>
    /// <param name="id">El ID de la persona que se edita.</param>
    /// <param name="personalAssets">Los bienes personales, o <see langword="null"/> si no tiene.</param>
    /// <returns>La persona con los datos editados.</returns>
    public static Person EditedPerson(Guid id, IList<PersonalAsset>? personalAssets = null)
    {
        Address address = Address.Create(
            StreetLine.Build("4441 Collins Avenue"),
            StreetLine2.Build("Apt 12B"),
            City.Build("Miami Beach"),
            State.Build("FL"),
            CountryName.Build("United States"),
            LatLng.Build(25.817887M, -80.122785M));

        Person person = Person.Load(
            id,
            address,
            FullName.Build("Roe, Jane"),
            Email.Build("janeroe@gmail.com"),
            PhoneNumber.Build("+1 305 555-0100"),
            Gender.Female,
            personalAssets,
            new DateTime(1990, 11, 25, 0, 0, 0, DateTimeKind.Utc));

        return person;
    }

    /// <summary>
    /// Crea una dirección nueva (con <see cref="Address.Create"/>, sin ID) con todos los datos.
    /// </summary>
    /// <returns>La dirección creada.</returns>
    public static Address NewAddress()
    {
        Address address = Address.Create(
            StreetLine.Build("Av. Corrientes 1234"),
            StreetLine2.Build("Piso 5, Depto B"),
            City.Build("Buenos Aires"),
            State.Build("CABA"),
            CountryName.Build("Argentina"),
            LatLng.Build(-34.603722M, -58.381592M));

        return address;
    }

    /// <summary>
    /// Carga una dirección existente (con <see cref="Address.Load"/>) con todos los datos.
    /// </summary>
    /// <param name="id">El ID de la dirección.</param>
    /// <returns>La dirección cargada.</returns>
    public static Address ExistingAddress(int id)
    {
        Address address = Address.Load(
            id,
            StreetLine.Build("Av. Corrientes 1234"),
            StreetLine2.Build("Piso 5, Depto B"),
            City.Build("Buenos Aires"),
            State.Build("CABA"),
            CountryName.Build("Argentina"),
            LatLng.Build(-34.603722M, -58.381592M));

        return address;
    }

    /// <summary>
    /// Crea un bien personal nuevo (con <see cref="PersonalAsset.Create"/>, sin ID).
    /// </summary>
    /// <param name="description">La descripción del bien.</param>
    /// <param name="value">El valor del bien.</param>
    /// <returns>El bien personal creado.</returns>
    public static PersonalAsset NewPersonalAsset(string description = "Notebook", decimal value = 1500M)
    {
        PersonalAsset personalAsset = PersonalAsset.Create(
            PersonalAssetDescription.Build(description),
            Money.Build(value));

        return personalAsset;
    }

    /// <summary>
    /// Carga un bien personal existente (con <see cref="PersonalAsset.Load"/>).
    /// </summary>
    /// <param name="id">El ID del bien.</param>
    /// <param name="description">La descripción del bien.</param>
    /// <param name="value">El valor del bien.</param>
    /// <returns>El bien personal cargado.</returns>
    public static PersonalAsset ExistingPersonalAsset(int id, string description = "Bicicleta", decimal value = 800M)
    {
        PersonalAsset personalAsset = PersonalAsset.Load(
            id,
            PersonalAssetDescription.Build(description),
            Money.Build(value));

        return personalAsset;
    }

    #endregion
}
