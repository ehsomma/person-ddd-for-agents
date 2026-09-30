using BuildingBlocks.Application.Cqrs;
using BuildingBlocks.Infra.Persistence.Abstractions;
using BuildingBlocks.Mediator.Abstractions;
using Microsoft.Extensions.Options;
using Records.Persons.Configuration;
using Records.Persons.Domain.PersonAggregate.Repositories;
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
    /// <param name="mediator">Implementation of mediator pattern to send and handle commands and queries implementing CQRS.</param>
    /// <param name="unitOfWork">Manage a <see cref="IDbSession"/> to encapsulate a business transaction which can affect the database.</param>
    /// <param name="settings">Represents the settings that will be mapped from the Persons key in the appsettings.</param>
    /// <param name="personRepository">Represents the repository for <see cref="DomainModel.Person"/>.</param>
    /// <exception cref="ArgumentNullException">When some argument for the constructor parameters is null.</exception>
    public CreatePersonCommandHandler(
        IMediator mediator,
        IUnitOfWork unitOfWork,
        IOptionsSnapshot<PersonsSettings> settings,
        IPersonRepository personRepository)
        : base(mediator, unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _settings = settings.Value;
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public override Task<Dto.Person> Handle(CreatePersonCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // NOTE: Just if we need some Persons settings.
        ////string setting1 = _settings.Setting1;


        _unitOfWork.BeginTransaction();
        _unitOfWork.Commit();


        return Task.FromResult(command.Person);
    }

    #endregion
}
