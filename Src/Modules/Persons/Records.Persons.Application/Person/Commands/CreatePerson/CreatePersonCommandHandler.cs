using BuildingBlocks.Application.Cqrs;
using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Domain.ValueObjects;
using BuildingBlocks.Infra.Persistence.Abstractions;
using Microsoft.Extensions.Options;
using Records.Persons.Configuration;
using Records.Persons.Domain.PersonAggregate.Enumerators;
using Records.Persons.Domain.PersonAggregate.Repositories;
using Records.Persons.Domain.PersonAggregate.ValueObjects;
using Records.Persons.Domain.Shared.ValueObjects;
using DomainModel = Records.Persons.Domain.PersonAggregate.Models; // Using aliases.
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Commands.CreatePerson;

/// <summary>
/// Represents a command handler for creating a new person..
/// </summary>
internal sealed class CreatePersonCommandHandler : CommandHandler<CreatePersonCommand, Dto.Person>
{
    #region Declarations

    private readonly PersonsSettings _settings;

    private readonly IPersonRepository _personRepository;

    #endregion

    #region Contructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePersonCommandHandler"/> class.
    /// </summary>
    /// <param name="domainEventPublisher">Publishes the domain events raised by the <see cref="DomainModel.Person"/> aggregate.</param>
    /// <param name="unitOfWork">Manage a <see cref="IDbSession"/> to encapsulate a business transaction which can affect the database.</param>
    /// <param name="settings">Represents the settings that will be mapped from the Persons key in the appsettings.</param>
    /// <param name="personRepository">Represents the repository for <see cref="DomainModel.Person"/>.</param>
    /// <exception cref="ArgumentNullException">When some argument for the constructor parameters is null.</exception>
    public CreatePersonCommandHandler(
        IDomainEventPublisher domainEventPublisher,
        IUnitOfWork unitOfWork,
        IOptionsSnapshot<PersonsSettings> settings,
        IPersonRepository personRepository)
        : base(domainEventPublisher, unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _settings = settings.Value;
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
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
        await ExecuteInTransaction(() =>
            _personRepository.InsertAsync(person));

        // Publica los eventos de dominio del agregado (p.ej. PersonCreated) recien despues del Commit,
        // para que los handlers solo reaccionen a cambios ya persistidos.
        await PublishDomainEvents(person.PullDomainEvents(), cancellationToken);

        return command.Person;
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
