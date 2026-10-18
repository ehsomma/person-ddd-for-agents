using BuildingBlocks.Application.Cqrs;
using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Application.Persistence.Abstractions;
using Records.Persons.Domain.PersonAggregate.Repositories;
using Records.Persons.Domain.PersonAggregate.Services;
using DomainModel = Records.Persons.Domain.PersonAggregate.Models; // Using aliases.

namespace Records.Persons.Application.Person.Commands.DeletePerson;

/// <summary>
/// Representa el command handler para eliminar una persona existente
/// (<see cref="DomainModel.Person"/>).
/// </summary>
internal sealed class DeletePersonCommandHandler : CommandHandler<DeletePersonCommand>
{
    #region Declarations

    private readonly IPersonRepository _personRepository;

    private readonly IPersonService _personService;

    #endregion

    #region Constructor

    /// <summary>Inicializa una nueva instancia de <see cref="DeletePersonCommandHandler"/>.</summary>
    /// <param name="domainEventPublisher">Publica los eventos de dominio que registra el aggregate <see cref="DomainModel.Person"/>.</param>
    /// <param name="unitOfWork">Maneja la <see cref="IDbSession"/> para ejecutar las operaciones que modifican la base dentro de una transacción.</param>
    /// <param name="personService">El servicio de dominio de <see cref="DomainModel.Person"/>.</param>
    /// <param name="personRepository">El repositorio de <see cref="DomainModel.Person"/>.</param>
    /// <exception cref="ArgumentNullException">Cuando alguno de los parámetros es <see langword="null"/>.</exception>
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

        // Carga el aggregate completo (no alcanza con chequear que exista): si no existe, el
        // servicio lanza DomainErrors.Person.NotFound, y es el aggregate el que decide si se puede
        // eliminar (Person.Delete aplica las reglas de negocio y registra el PersonDeletedEvent).
        DomainModel.Person person = await _personService.GetByIdAsync(command.Id, cancellationToken);

        person.Delete();

        // Ejecuta la operación dentro de una transacción de base de datos: si sale bien hace
        // commit, y si falla hace rollback y relanza la excepción.
        await ExecuteInTransaction(
            () => _personRepository.DeleteAsync(person, cancellationToken),
            cancellationToken);

        // Publica los eventos de dominio del aggregate (p.ej. PersonDeletedEvent) recién después
        // del commit, para que los handlers solo reaccionen a cambios ya persistidos.
        await PublishDomainEvents(person.PullDomainEvents());
    }

    #endregion
}
