using BuildingBlocks.Application.Cqrs;
using BuildingBlocks.Application.Persistence.Abstractions;
using BuildingBlocks.Domain.Exceptions;
using Records.Persons.Domain.PersonAggregate.Errors;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersonById;

/// <summary>
/// Representa el query handler que obtiene una persona por su ID (<see
/// cref="GetPersonByIdQuery"/>).
/// </summary>
internal sealed class GetPersonByIdQueryHandler : QueryHandler<GetPersonByIdQuery, Dto.Person>
{
    #region Declarations

    private readonly IQueryRepository<GetPersonByIdQuery, Dto.Person?> _getPersonByIdRepository;

    #endregion

    #region Constructor

    /// <summary>Inicializa una nueva instancia de <see cref="GetPersonByIdQueryHandler"/>.</summary>
    /// <param name="getPersonByIdRepository">El query repository que lee la persona (<see cref="Dto.Person"/>), o <see langword="null"/> si no existe.</param>
    /// <exception cref="ArgumentNullException">Cuando alguno de los parámetros es <see langword="null"/>.</exception>
    public GetPersonByIdQueryHandler(IQueryRepository<GetPersonByIdQuery, Dto.Person?> getPersonByIdRepository)
    {
        _getPersonByIdRepository = getPersonByIdRepository ?? throw new ArgumentNullException(nameof(getPersonByIdRepository));
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    /// <exception cref="DomainException">Cuando la persona no existe (<see cref="DomainErrors.Person.NotFound"/>).</exception>
    public override async Task<Dto.Person> Handle(GetPersonByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Si no existe, lanza el mismo error que los commands (ERR.PERSON.NOTFOUND → 404), así la
        // API responde igual en los dos casos.
        Dto.Person person = await _getPersonByIdRepository.GetAsync(query, cancellationToken)
            ?? throw new DomainException(DomainErrors.Person.NotFound);

        return person;
    }

    #endregion
}
