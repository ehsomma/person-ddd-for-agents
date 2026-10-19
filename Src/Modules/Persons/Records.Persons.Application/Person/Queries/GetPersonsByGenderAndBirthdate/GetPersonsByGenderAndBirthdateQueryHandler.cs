using BuildingBlocks.Application.Cqrs;
using BuildingBlocks.Application.Persistence.Abstractions;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersonsByGenderAndBirthdate;

/// <summary>
/// Representa el handler de la <see cref="GetPersonsByGenderAndBirthdateQuery"/>.
/// </summary>
internal sealed class GetPersonsByGenderAndBirthdateQueryHandler : QueryHandler<GetPersonsByGenderAndBirthdateQuery, IList<Dto.Person>>
{
    #region Declarations

    private readonly IQueryRepository<GetPersonsByGenderAndBirthdateQuery, IList<Dto.Person>> _getPersonsByGenderAndBirthdateRepository;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="GetPersonsByGenderAndBirthdateQueryHandler"/>.
    /// </summary>
    /// <param name="getPersonsByGenderAndBirthdateRepository">Representa el repositorio de la query que lee la lista de <see cref="Dto.Person"/>.</param>
    /// <exception cref="ArgumentNullException">Cuando algún argumento de los parámetros del constructor es nulo.</exception>
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
