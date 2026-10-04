using BuildingBlocks.Application.Cqrs;
using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Application.Mappings.Abstractions.Mappers;
using BuildingBlocks.Domain.ValueObjects;
using BuildingBlocks.Infra.Persistence.Abstractions;
using Records.Persons.Domain.PersonAggregate.Enumerators;
using Records.Persons.Domain.PersonAggregate.Repositories;
using Records.Persons.Domain.PersonAggregate.Services;
using Records.Persons.Domain.PersonAggregate.ValueObjects;
using Records.Persons.Domain.Shared.ValueObjects;
using DomainModel = Records.Persons.Domain.PersonAggregate.Models; // Using aliases.
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Commands.UpdatePerson;

/// <summary>
/// Represents a command handler for updating an existing person.
/// </summary>
internal sealed class UpdatePersonCommandHandler : CommandHandler<UpdatePersonCommand, Dto.Person>
{
    #region Declarations

    private readonly IPersonRepository _personRepository;

    private readonly IPersonService _personService;

    private readonly IDomainMapper<DomainModel.Person, Dto.Person> _personMapper;

    #endregion

    #region Contructor

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePersonCommandHandler"/> class.
    /// </summary>
    /// <param name="domainEventPublisher">Publishes the domain events raised by the <see cref="DomainModel.Person"/> aggregate.</param>
    /// <param name="unitOfWork">Manage a <see cref="IDbSession"/> to encapsulate a business transaction which can affect the database.</param>
    /// <param name="personService">Represents the service for <see cref="DomainModel.Person"/>.</param>
    /// <param name="personRepository">Represents the repository for <see cref="DomainModel.Person"/>.</param>
    /// <param name="personMapper">Defines a mapper to map a <see cref="DomainModel.Person"/> to a <see cref="Dto.Person"/>.</param>
    /// <exception cref="ArgumentNullException">When some argument for the constructor parameters is null.</exception>
    public UpdatePersonCommandHandler(
        IDomainEventPublisher domainEventPublisher,
        IUnitOfWork unitOfWork,
        IPersonService personService,
        IPersonRepository personRepository,
        IDomainMapper<DomainModel.Person, Dto.Person> personMapper)
        : base(domainEventPublisher, unitOfWork)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
        _personMapper = personMapper ?? throw new ArgumentNullException(nameof(personMapper));
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public override async Task<Dto.Person> Handle(UpdatePersonCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Loads a DomainModel.Person with the edited data coming from the command (FromCommandToDomain).
        DomainModel.Person editedPerson = LoadPerson(command);

        // Gets the current DomainModel.Person.
        DomainModel.Person currentPerson = await _personService.GetByIdAsync(editedPerson.Id);

        // Updates the currentPerson with editedPerson data.
        currentPerson.Update(editedPerson);

        // Ejecuta la `operación` especificada dentro de una transacción de base de datos, hace commit
        // de los cambios si tiene éxito o rollback y relanza la excepción si falla.
        await ExecuteInTransaction(() =>
            _personRepository.UpdateAsync(currentPerson));

        // Publica los eventos de dominio del agregado (p.ej. PersonUpdated) recien despues del Commit,
        // para que los handlers solo reaccionen a cambios ya persistidos.
        await PublishDomainEvents(currentPerson.PullDomainEvents(), cancellationToken);

        // Reads the person again to return what was actually persisted (e.g. the IDs generated for
        // the new personal assets, which the domain model doesn't receive from the insert).
        DomainModel.Person updatedPerson = await _personService.GetByIdAsync(currentPerson.Id);

        Dto.Person updatedPersonDto = _personMapper.FromDomainToDto(updatedPerson);

        return updatedPersonDto;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Carga un <see cref="DomainModel.Person"/> con los datos editados del `command` especificado.
    /// </summary>
    /// <param name="command">El comando que contiene los datos editados del <see cref="DomainModel.Person"/>.</param>
    /// <returns>Un <see cref="DomainModel.Person"/>.</returns>
    private DomainModel.Person LoadPerson(UpdatePersonCommand command)
    {
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

            domainAddress = DomainModel.Address.Load(
                personAddressDto.Id,
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

        DomainModel.Person person = DomainModel.Person.Load(
            personDto.Id,
            domainAddress,
            fullName,
            email,
            phoneNumber,
            Gender.FromName(personDto.Gender),
            null,
            personDto.Birthdate);

        // Loaded as they come (the new ones without ID): Person.Update decides which ones to add.
        if (personDto.PersonalAssets != null)
        {
            foreach (Dto.PersonalAsset personalAssetDto in personDto.PersonalAssets)
            {
                DomainModel.PersonalAsset personalAsset = DomainModel.PersonalAsset.Load(
                    personalAssetDto.Id,
                    PersonalAssetDescription.Build(personalAssetDto.Description),
                    Money.Build(personalAssetDto.Value));
                person.LoadPersonalAsset(personalAsset);
            }
        }

        return person;
    }

    #endregion
}
