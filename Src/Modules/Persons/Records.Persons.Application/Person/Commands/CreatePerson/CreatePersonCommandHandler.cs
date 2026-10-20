using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Application.Cqrs;
using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Application.Mappings.Abstractions.Mappers;
using BuildingBlocks.Application.Persistence.Abstractions;
using BuildingBlocks.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using Records.Persons.Configuration;
using Records.Persons.Domain.PersonAggregate.Enumerators;
using Records.Persons.Domain.PersonAggregate.Repositories;
using Records.Persons.Domain.PersonAggregate.Services;
using Records.Persons.Domain.PersonAggregate.ValueObjects;
using Records.Persons.Domain.Shared.ValueObjects;
using DomainModel = Records.Persons.Domain.PersonAggregate.Models; // Using aliases.
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Commands.CreatePerson;

/// <summary>
/// Representa el command handler para crear una nueva persona.
/// </summary>
internal sealed class CreatePersonCommandHandler : CommandHandler<CreatePersonCommand, Dto.Person>
{
    #region Declarations

    // ReSharper disable once NotAccessedField.Local
    [SuppressMessage(
        "CodeQuality",
        "IDE0052:Remove unread private members",
        Justification = "Not used, kept as an example of how to inject settings into a handler.")]
    private readonly PersonsSettings _settings; // NOTE: No se usa pero se deja como ejemplo de como inyectar settings en un handler.

    private readonly IPersonRepository _personRepository;

    private readonly IPersonService _personService;

    private readonly IDomainMapper<DomainModel.Person, Dto.Person> _personMapper;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CreatePersonCommandHandler"/>.
    /// </summary>
    /// <param name="domainEventPublisher">Publica los eventos de dominio generados por el agregado <see cref="DomainModel.Person"/>.</param>
    /// <param name="unitOfWork">Gestiona un <see cref="IDbSession"/> para encapsular una transacción de negocio que puede afectar la base de datos.</param>
    /// <param name="settings">Representa la configuración que se mapeará desde la clave Persons en los appsettings.</param>
    /// <param name="personRepository">Representa el repositorio para <see cref="DomainModel.Person"/>.</param>
    /// <param name="personService">Representa el servicio para <see cref="DomainModel.Person"/>.</param>
    /// <param name="personMapper">Define un mapeador para mapear un <see cref="DomainModel.Person"/> a un <see cref="Dto.Person"/>.</param>
    /// <exception cref="ArgumentNullException">Cuando algún argumento de los parámetros del constructor es nulo.</exception>
    public CreatePersonCommandHandler(
        IDomainEventPublisher domainEventPublisher,
        IUnitOfWork unitOfWork,
        IOptionsSnapshot<PersonsSettings> settings,
        IPersonRepository personRepository,
        IPersonService personService,
        IDomainMapper<DomainModel.Person, Dto.Person> personMapper)
        : base(domainEventPublisher, unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _settings = settings.Value;
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
        _personMapper = personMapper ?? throw new ArgumentNullException(nameof(personMapper));
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public override async Task<Dto.Person> Handle(CreatePersonCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // NOTE: Just if we need some Persons settings.
        ////string setting1 = _settings.Setting1;

        DomainModel.Person person = CreatePerson(command);

        // Ejecuta la `operación` especificada dentro de una transacción de base de datos, hace commit
        // de los cambios si tiene éxito o rollback y relanza la excepción si falla.
        await ExecuteInTransaction(
            () => _personRepository.InsertAsync(person, cancellationToken),
            cancellationToken);

        // Publica los eventos de dominio del agregado (p.ej. PersonCreated) recién después del Commit,
        // para que los handlers solo reaccionen a cambios ya persistidos.
        await PublishDomainEvents(person.PullDomainEvents());

        // Reads the person again to return what was actually persisted (e.g. the IDs generated for
        // the address and the personal assets, which the domain model doesn't receive from the insert).
        DomainModel.Person createdPerson = await _personService.GetByIdAsync(person.Id, cancellationToken);

        Dto.Person createdPersonDto = _personMapper.FromDomainToDto(createdPerson);

        return createdPersonDto;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Crea un nuevo <see cref="DomainModel.Person"/> con los datos del `command` especificado.
    /// </summary>
    /// <param name="command">El comando que contiene los datos para crear el <see cref="DomainModel.Person"/>.</param>
    /// <returns>Un <see cref="DomainModel.Person"/>.</returns>
    private DomainModel.Person CreatePerson(CreatePersonCommand command)
    {
        ////ArgumentNullException.ThrowIfNull(command);

        Dto.Person personDto = command.Person;
        Dto.Address? personAddressDto = command.Person.Address;

        DomainModel.Address? domainAddress = null;
        if (personAddressDto != null)
        {
            LatLng? domainLatLng = null;
            if (personAddressDto.LatLng != null)
            {
                domainLatLng = LatLng.Build(personAddressDto.LatLng.Lat, personAddressDto.LatLng.Lng);
            }

            domainAddress = DomainModel.Address.Create(
                StreetLine.Build(personAddressDto.StreetLine1),
                StreetLine2.Build(personAddressDto.StreetLine2),
                City.Build(personAddressDto.City),
                State.Build(personAddressDto.State),
                CountryName.Build(personAddressDto.Country),
                domainLatLng);
        }

        FullName fullName = FullName.Build(personDto.FullName);
        Email email = Email.Build(personDto.Email);
        PhoneNumber phoneNumber = PhoneNumber.Build(personDto.Phone);

        DomainModel.Person person = DomainModel.Person.Create(
            personDto.Id,
            domainAddress,
            fullName,
            email,
            phoneNumber,
            Gender.FromName(personDto.Gender),
            null,
            personDto.Birthdate);

        if (personDto.PersonalAssets != null)
        {
            foreach (Dto.PersonalAsset personalAssetDto in personDto.PersonalAssets)
            {
                DomainModel.PersonalAsset domainPersonalAsset = DomainModel.PersonalAsset.Create(
                    PersonalAssetDescription.Build(personalAssetDto.Description),
                    Money.Build(personalAssetDto.Value));
                person.AddPersonalAsset(domainPersonalAsset);
            }
        }

        return person;
    }

    #endregion
}
