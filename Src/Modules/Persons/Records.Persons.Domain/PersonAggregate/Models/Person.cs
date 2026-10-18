using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.ValueObjects;
using Records.Persons.Domain.PersonAggregate.Enumerators;
using Records.Persons.Domain.PersonAggregate.Events;
using Records.Persons.Domain.PersonAggregate.ValueObjects;
using Throw;

namespace Records.Persons.Domain.PersonAggregate.Models;

/// <summary>
/// Representa una persona. Es el aggregate root del aggregate de personas, que también incluye su
/// dirección (<see cref="Models.Address"/>) y sus bienes personales (<see cref="PersonalAsset"/>).
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "StyleCop.CSharp.OrderingRules",
    "SA1201:Elements should appear in the correct order",
    Justification = "Create() y Load() van dentro de la region Constructor.")]
public sealed class Person : AggregateRoot<Guid>, IEntityAuditable
{
    #region Declarations

    // NOTE: Es un field privado (y no una lista pública) para proteger el aggregate: desde afuera
    // no se pueden agregar bienes personales directo a la colección, solo a través de los métodos
    // del aggregate (AddPersonalAsset, LoadPersonalAsset) o del constructor, que son los que
    // aplican las reglas y registran los eventos.
    private readonly IList<PersonalAsset> _personalAssets;

    #endregion

    #region Constructor

    /// <summary>Inicializa una nueva instancia de <see cref="Person"/> con los datos especificados.</summary>
    /// <param name="id">El ID de la persona.</param>
    /// <param name="address">La dirección (<see cref="Models.Address"/>).</param>
    /// <param name="fullName">El nombre completo (<see cref="ValueObjects.FullName"/>).</param>
    /// <param name="email">El email.</param>
    /// <param name="phone">El teléfono (<see cref="PhoneNumber"/>).</param>
    /// <param name="gender">El género (<see cref="Enumerators.Gender"/>).</param>
    /// <param name="personalAssets">Los bienes personales (<see cref="PersonalAsset"/>).</param>
    /// <param name="birthdate">La fecha de nacimiento.</param>
    /// <param name="createdOnUtc">La fecha y hora (UTC) en que se creó la entidad.</param>
    /// <param name="updatedOnUtc">La fecha y hora (UTC) en que se modificó la entidad por última vez.</param>
    private Person(
        Guid id,
        Address? address,
        FullName? fullName,
        Email? email,
        PhoneNumber? phone,
        Gender? gender,
        IList<PersonalAsset>? personalAssets,
        DateTime? birthdate,
        DateTime? createdOnUtc,
        DateTime? updatedOnUtc)
        : base(id)
    {
        EnsureMinimalRequiredData(address, fullName, email, gender);

        Id = id;
        Address = address!; // Validado en EnsureMinimalRequiredData.
        FullName = fullName!; // Validado en EnsureMinimalRequiredData.
        Email = email!; // Validado en EnsureMinimalRequiredData.
        Phone = phone!; // Puede ser null: el teléfono es opcional.
        Gender = gender!; // Validado en EnsureMinimalRequiredData.
        Birthdate = birthdate;
        _personalAssets = personalAssets ?? new List<PersonalAsset>();
        CreatedOnUtc = createdOnUtc;
        UpdatedOnUtc = updatedOnUtc;
    }

    /// <summary>
    /// Crea una persona nueva: valida los datos mínimos y registra el
    /// <see cref="PersonCreatedEvent"/>. Es la forma de crear una persona que todavía no existe.
    /// </summary>
    /// <param name="id">El ID de la persona.</param>
    /// <param name="address">La dirección (<see cref="Models.Address"/>).</param>
    /// <param name="fullName">El nombre completo (<see cref="ValueObjects.FullName"/>).</param>
    /// <param name="email">El email.</param>
    /// <param name="phone">El teléfono (<see cref="PhoneNumber"/>).</param>
    /// <param name="gender">El género (<see cref="Enumerators.Gender"/>).</param>
    /// <param name="personalAssets">Los bienes personales (<see cref="PersonalAsset"/>).</param>
    /// <param name="birthdate">La fecha de nacimiento.</param>
    /// <returns>La persona creada.</returns>
    public static Person Create(
        Guid id,
        Address? address,
        FullName? fullName,
        Email? email,
        PhoneNumber? phone,
        Gender? gender,
        IList<PersonalAsset>? personalAssets,
        DateTime? birthdate)
    {
        DateTime utcNow = DateTime.UtcNow;

        Person person = new Person(id, address, fullName, email, phone, gender, personalAssets, birthdate, utcNow, utcNow);

        person.RegisterDomainEvent(new PersonCreatedEvent(person));

        return person;
    }

    /// <summary>
    /// Rehidrata una persona que ya existe (p.ej. leída de la base): valida los datos mínimos pero
    /// no registra eventos, porque no es un cambio sino la reconstrucción de algo ya persistido.
    /// </summary>
    /// <param name="id">El ID de la persona.</param>
    /// <param name="address">La dirección (<see cref="Models.Address"/>).</param>
    /// <param name="fullName">El nombre completo (<see cref="ValueObjects.FullName"/>).</param>
    /// <param name="email">El email.</param>
    /// <param name="phone">El teléfono (<see cref="PhoneNumber"/>).</param>
    /// <param name="gender">El género (<see cref="Enumerators.Gender"/>).</param>
    /// <param name="personalAssets">Los bienes personales (<see cref="PersonalAsset"/>).</param>
    /// <param name="birthdate">La fecha de nacimiento.</param>
    /// <param name="createdOnUtc">La fecha y hora (UTC) en que se creó la entidad.</param>
    /// <param name="updatedOnUtc">La fecha y hora (UTC) en que se modificó la entidad por última vez.</param>
    /// <returns>La persona rehidratada.</returns>
    public static Person Load(
        Guid id,
        Address? address,
        FullName? fullName,
        Email? email,
        PhoneNumber? phone,
        Gender? gender,
        IList<PersonalAsset>? personalAssets,
        DateTime? birthdate,
        DateTime? createdOnUtc = null,
        DateTime? updatedOnUtc = null)
    {
        Person person = new Person(id, address, fullName, email, phone, gender, personalAssets, birthdate, createdOnUtc, updatedOnUtc);

        return person;
    }

    #endregion

    #region Properties

    /// <summary>La dirección.</summary>
    public Address Address { get; private set; }

    /// <summary>El nombre completo.</summary>
    public FullName FullName { get; private set; }

    /// <summary>El email.</summary>
    public Email Email { get; private set; }

    /// <summary>El teléfono (opcional).</summary>
    public PhoneNumber? Phone { get; private set; }

    /// <summary>El género.</summary>
    public Gender Gender { get; private set; }

    /// <summary>La fecha de nacimiento (opcional).</summary>
    public DateTime? Birthdate { get; private set; }

    /// <summary>Los bienes personales (<see cref="PersonalAsset"/>), de solo lectura.</summary>
    public IReadOnlyCollection<PersonalAsset> PersonalAssets => (IReadOnlyCollection<PersonalAsset>)_personalAssets;

    /// <inheritdoc />
    public DateTime? CreatedOnUtc { get; }

    /// <inheritdoc />
    public DateTime? UpdatedOnUtc { get; private set; }

    #endregion

    #region Public methods

    /// <summary>
    /// Actualiza esta persona con los datos de <paramref name="updatedData"/>: reemplaza los datos
    /// personales y la dirección, agrega los bienes personales nuevos y registra el
    /// <see cref="PersonUpdatedEvent"/>.
    /// </summary>
    /// <param name="updatedData">La persona con los datos editados.</param>
    public void Update(Person updatedData)
    {
        // Una Person ya construida nunca es inválida, pero un command handler podría pasar null
        // como parámetro.
        updatedData.ThrowIfNull();

        FullName = updatedData.FullName;
        Email = updatedData.Email;
        Phone = updatedData.Phone;
        Gender = updatedData.Gender;
        Birthdate = updatedData.Birthdate;
        UpdatedOnUtc = DateTime.UtcNow;

        UpdateAddress(updatedData.Address);

        RegisterDomainEvent(new PersonUpdatedEvent(this));

        // Solo agrega los bienes personales nuevos (los que todavía no tienen ID, ver
        // PersonalAsset.Create()): los existentes no se actualizan ni se eliminan desde acá.
        //   Id = 0  → nuevo: se agrega y registra un PersonalAssetAddedEvent ✓
        //   Id = 15 → ya existe: se ignora (no se pisa con los datos que vienen)
        foreach (PersonalAsset personalAsset in updatedData.PersonalAssets.Where(personalAsset => personalAsset.Id == 0))
        {
            AddPersonalAsset(personalAsset);
        }

        // TODO: Actualizar los bienes personales existentes con su propio Update, no desde acá.
        ////_personalAssets = ...
    }

    /// <summary>
    /// Elimina esta persona: registra el <see cref="PersonDeletedEvent"/>. El borrado en la base lo
    /// hace el repositorio cuando el command handler persiste el cambio.
    /// </summary>
    public void Delete()
    {
        // Acá van las reglas de negocio que impidan eliminar la persona (p.ej. que tenga
        // operaciones pendientes). Si el borrado fuese lógico, acá también se actualizaría el
        // estado.

        RegisterDomainEvent(new PersonDeletedEvent(this));
    }

    /// <summary>
    /// Agrega un bien personal (<see cref="PersonalAsset"/>) nuevo a la persona y registra el
    /// <see cref="PersonalAssetAddedEvent"/>.
    /// </summary>
    /// <param name="personalAsset">El bien personal a agregar.</param>
    public void AddPersonalAsset(PersonalAsset personalAsset)
    {
        _personalAssets.Add(personalAsset);

        RegisterDomainEvent(new PersonalAssetAddedEvent(this, personalAsset));
    }

    /// <summary>
    /// Carga un bien personal (<see cref="PersonalAsset"/>) que ya existe (p.ej. leído de la base),
    /// sin registrar eventos. Es el equivalente de <see cref="Load"/> para los bienes personales.
    /// </summary>
    /// <param name="personalAsset">El bien personal a cargar.</param>
    public void LoadPersonalAsset(PersonalAsset personalAsset)
    {
        _personalAssets.Add(personalAsset);
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Valida que estén los datos mínimos obligatorios: la dirección, el nombre completo, el email
    /// y el género. El teléfono y la fecha de nacimiento son opcionales.
    /// </summary>
    /// <param name="address">La dirección.</param>
    /// <param name="fullName">El nombre completo.</param>
    /// <param name="email">El email.</param>
    /// <param name="gender">El género.</param>
    /// <exception cref="DomainValidationException">Cuando falta alguno de los datos obligatorios.</exception>
    private void EnsureMinimalRequiredData(Address? address, FullName? fullName, Email? email, Gender? gender)
    {
        try
        {
            address.ThrowIfNull();
            fullName.ThrowIfNull();
            email.ThrowIfNull();
            ////phone.ThrowIfNull(); // Puede ser null.
            gender.ThrowIfNull();
            ////birthdate.ThrowIfNull(); // Puede ser null.
        }
        catch (Exception ex)
        {
            throw new DomainValidationException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Reemplaza la dirección por una con los datos de <paramref name="updatedAddress"/>, pero con
    /// el ID de la dirección actual.
    /// </summary>
    /// <param name="updatedAddress">La dirección con los datos editados.</param>
    private void UpdateAddress(Address updatedAddress)
    {
        Address = Address.Load(
            Address.Id, // Se conserva el ID: es la misma dirección con otros datos.
            updatedAddress.StreetLine1,
            updatedAddress.StreetLine2,
            updatedAddress.City,
            updatedAddress.State,
            updatedAddress.Country,
            updatedAddress.LatLng);
    }

    #endregion
}
