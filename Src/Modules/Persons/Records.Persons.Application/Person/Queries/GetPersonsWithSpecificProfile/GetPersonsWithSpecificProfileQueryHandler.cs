using BuildingBlocks.Application.Cqrs;
using BuildingBlocks.Infra.Persistence.Abstractions;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersonsWithSpecificProfile;

/// <summary>
/// Represents the <see cref="GetPersonsWithSpecificProfileQuery"/> handler.
/// </summary>
internal sealed class GetPersonsWithSpecificProfileQueryHandler : QueryHandler<GetPersonsWithSpecificProfileQuery, IList<Dto.Person>>
{
    #region Declarations

    private readonly IQueryRepository<GetPersonsWithSpecificProfileQuery, IList<Dto.Person>> _getPersonsWithSpecificProfileRepository;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPersonsWithSpecificProfileQueryHandler"/> class.
    /// </summary>
    /// <param name="getPersonsWithSpecificProfileRepository">Represents the query repository that reads the <see cref="Dto.Person"/> list.</param>
    /// <exception cref="ArgumentNullException">When some argument for the constructor parameters is null.</exception>
    public GetPersonsWithSpecificProfileQueryHandler(
        IQueryRepository<GetPersonsWithSpecificProfileQuery, IList<Dto.Person>> getPersonsWithSpecificProfileRepository)
    {
        _getPersonsWithSpecificProfileRepository = getPersonsWithSpecificProfileRepository
            ?? throw new ArgumentNullException(nameof(getPersonsWithSpecificProfileRepository));
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public override async Task<IList<Dto.Person>> Handle(GetPersonsWithSpecificProfileQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        IList<Dto.Person> persons = await _getPersonsWithSpecificProfileRepository.GetAsync(query, cancellationToken);
        return persons;
    }

    #endregion
}
