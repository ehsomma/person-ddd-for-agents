using BuildingBlocks.Application.Cqrs;
using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Infra.Persistence.Abstractions;
using Records.Persons.Domain.PersonAggregate.Repositories;
using Records.Persons.Domain.PersonAggregate.Services;
using DomainModel = Records.Persons.Domain.PersonAggregate.Models; // Using aliases.

namespace Records.Persons.Application.Person.Commands.DeletePerson;

/// <summary>
/// Represents a command handler for deleting an existing person.
/// </summary>
internal sealed class DeletePersonCommandHandler : CommandHandler<DeletePersonCommand>
{
    #region Declarations

    private readonly IPersonRepository _personRepository;

    private readonly IPersonService _personService;

    #endregion

    #region Contructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePersonCommandHandler"/> class.
    /// </summary>
    /// <param name="domainEventPublisher">Publishes the domain events raised by the <see cref="DomainModel.Person"/> aggregate.</param>
    /// <param name="unitOfWork">Manage a <see cref="IDbSession"/> to encapsulate a business transaction which can affect the database.</param>
    /// <param name="personService">Represents the service for <see cref="DomainModel.Person"/>.</param>
    /// <param name="personRepository">Represents the repository for <see cref="DomainModel.Person"/>.</param>
    /// <exception cref="ArgumentNullException">When some argument for the constructor parameters is null.</exception>
    public DeletePersonCommandHandler(
        IDomainEventPublisher domainEventPublisher,
        IUnitOfWork unitOfWork,
        IPersonService personService,
        IPersonRepository personRepository)
        : base(domainEventPublisher, unitOfWork)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public override async Task Handle(DeletePersonCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Gets the whole aggregate (not just an "exists" check): if it doesn't exist the service throws
        // DomainErrors.Person.NotFound, and the aggregate is the one that decides whether it can be
        // deleted (Person.Delete applies the business rules and registers PersonDeleted).
        DomainModel.Person person = await _personService.GetByIdAsync(command.Id);

        person.Delete();

        // Ejecuta la `operación` especificada dentro de una transacción de base de datos, hace commit
        // de los cambios si tiene éxito o rollback y relanza la excepción si falla.
        await ExecuteInTransaction(() =>
            _personRepository.DeleteAsync(person));

        // Publica los eventos de dominio del agregado (p.ej. PersonDeleted) recien despues del Commit,
        // para que los handlers solo reaccionen a cambios ya persistidos.
        await PublishDomainEvents(person.PullDomainEvents(), cancellationToken);
    }

    #endregion
}
