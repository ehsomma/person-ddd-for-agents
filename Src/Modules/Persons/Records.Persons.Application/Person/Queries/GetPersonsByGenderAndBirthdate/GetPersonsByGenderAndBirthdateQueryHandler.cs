using BuildingBlocks.Application.Cqrs;
using BuildingBlocks.Infra.Persistence.Abstractions;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersonsByGenderAndBirthdate;

/// <summary>
/// Represents the <see cref="GetPersonsByGenderAndBirthdateQuery"/> handler.
/// </summary>
internal sealed class GetPersonsByGenderAndBirthdateQueryHandler : QueryHandler<GetPersonsByGenderAndBirthdateQuery, IList<Dto.Person>>
{
    #region Declarations

    private readonly IQueryRepository<GetPersonsByGenderAndBirthdateQuery, IList<Dto.Person>> _getPersonsByGenderAndBirthdateRepository;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPersonsByGenderAndBirthdateQueryHandler"/> class.
    /// </summary>
    /// <param name="getPersonsByGenderAndBirthdateRepository">Represents the query repository that reads the <see cref="Dto.Person"/> list.</param>
    /// <exception cref="ArgumentNullException">When some argument for the constructor parameters is null.</exception>
    public GetPersonsByGenderAndBirthdateQueryHandler(
        IQueryRepository<GetPersonsByGenderAndBirthdateQuery, IList<Dto.Person>> getPersonsByGenderAndBirthdateRepository)
    {
        _getPersonsByGenderAndBirthdateRepository = getPersonsByGenderAndBirthdateRepository
            ?? throw new ArgumentNullException(nameof(getPersonsByGenderAndBirthdateRepository));
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public override async Task<IList<Dto.Person>> Handle(GetPersonsByGenderAndBirthdateQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        IList<Dto.Person> persons = await _getPersonsByGenderAndBirthdateRepository.GetAsync(query, cancellationToken);
        return persons;
    }

    #endregion
}
