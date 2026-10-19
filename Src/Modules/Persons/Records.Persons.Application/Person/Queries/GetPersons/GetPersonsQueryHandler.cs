using BuildingBlocks.Application.Cqrs;
using BuildingBlocks.Application.Persistence.Abstractions;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersons;

/// <summary>
/// Representa el handler de la <see cref="GetPersonsQuery"/>.
/// </summary>
internal sealed class GetPersonsQueryHandler : QueryHandler<GetPersonsQuery, IList<Dto.Person>>
{
    #region Declarations

    private readonly IQueryRepository<GetPersonsQuery, IList<Dto.Person>> _getPersonsRepository;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="GetPersonsQueryHandler"/>.
    /// </summary>
    /// <param name="getPersonsRepository">Representa el repositorio de la query que lee la lista de <see cref="Dto.Person"/>.</param>
    /// <exception cref="ArgumentNullException">Cuando algún argumento de los parámetros del constructor es nulo.</exception>
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
