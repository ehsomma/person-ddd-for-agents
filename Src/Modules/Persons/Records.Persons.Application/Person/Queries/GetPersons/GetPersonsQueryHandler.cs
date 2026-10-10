using BuildingBlocks.Application.Cqrs;
using BuildingBlocks.Infra.Persistence.Abstractions;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersons;

/// <summary>
/// Represents the <see cref="GetPersonsQuery"/> handler.
/// </summary>
internal sealed class GetPersonsQueryHandler : QueryHandler<GetPersonsQuery, IList<Dto.Person>>
{
    #region Declarations

    private readonly IQueryRepository<GetPersonsQuery, IList<Dto.Person>> _getPersonsRepository;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPersonsQueryHandler"/> class.
    /// </summary>
    /// <param name="getPersonsRepository">Represents the query repository that reads the <see cref="Dto.Person"/> list.</param>
    /// <exception cref="ArgumentNullException">When some argument for the constructor parameters is null.</exception>
    public GetPersonsQueryHandler(IQueryRepository<GetPersonsQuery, IList<Dto.Person>> getPersonsRepository)
    {
        _getPersonsRepository = getPersonsRepository ?? throw new ArgumentNullException(nameof(getPersonsRepository));
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public override async Task<IList<Dto.Person>> Handle(GetPersonsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        IList<Dto.Person> persons = await _getPersonsRepository.GetAsync(query, cancellationToken);
        return persons;
    }

    #endregion
}
