using BuildingBlocks.Application.Cqrs;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Infra.Persistence.Abstractions;
using Records.Persons.Domain.PersonAggregate.Errors;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersonById;

/// <summary>
/// Represents the <see cref="GetPersonByIdQuery"/> handler.
/// </summary>
internal sealed class GetPersonByIdQueryHandler : QueryHandler<GetPersonByIdQuery, Dto.Person>
{
    #region Declarations

    private readonly IQueryRepository<GetPersonByIdQuery, Dto.Person?> _getPersonByIdRepository;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPersonByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="getPersonByIdRepository">
    /// Represents the query repository that reads the <see cref="Dto.Person"/> (<see langword="null"/> if it doesn't exist).
    /// </param>
    /// <exception cref="ArgumentNullException">When some argument for the constructor parameters is null.</exception>
    public GetPersonByIdQueryHandler(IQueryRepository<GetPersonByIdQuery, Dto.Person?> getPersonByIdRepository)
    {
        _getPersonByIdRepository = getPersonByIdRepository ?? throw new ArgumentNullException(nameof(getPersonByIdRepository));
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    /// <exception cref="DomainException">When the person doesn't exist (<see cref="DomainErrors.Person.NotFound"/>).</exception>
    public override async Task<Dto.Person> Handle(GetPersonByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Not found: same error as the commands (ERR.PERSON.NOTFOUND -> 404), so the API answers the same way.
        Dto.Person person = await _getPersonByIdRepository.GetAsync(query, cancellationToken)
            ?? throw new DomainException(DomainErrors.Person.NotFound);

        return person;
    }

    #endregion
}
