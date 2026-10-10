using System.Data.Common;
using BuildingBlocks.Infra.Persistence;
using Dapper;
using Microsoft.Extensions.Configuration;
using Records.Persons.Application.Person.Queries.GetPersons;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Infra.Queries.Sql.Person.GetPersons;

/// <summary>
/// Represents the query repository that reads the full list of <see cref="Dto.Person"/> (<see cref="GetPersonsQuery"/>).
/// Dapper maps the result directly to the DTOs (no data model, no mappers).
/// </summary>
public sealed class GetPersonsRepository : QueryRepository<GetPersonsQuery, IList<Dto.Person>>
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPersonsRepository"/> class.
    /// </summary>
    /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
    public GetPersonsRepository(IConfiguration configuration)
        : base(configuration)
    {
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public override async Task<IList<Dto.Person>> GetAsync(GetPersonsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        // The persons, their addresses and their personal assets in a single round trip (one row per personal asset).
        // NOTE: The column order is the one expected by QueryPersonsAsync (it defines the splitOn).
        // TODO: Implement a paged list.
        const string sql = """
                           SELECT PE.[Id]
                               ,PE.[FullName]
                               ,PE.[Email]
                               ,PE.[Phone]
                               ,PE.[Gender]
                               ,PE.[Birthdate]
                               ,AD.[Id]
                               ,AD.[StreetLine1]
                               ,AD.[StreetLine2]
                               ,AD.[City]
                               ,AD.[State]
                               ,AD.[Country]
                               ,AD.[Lat]
                               ,AD.[Lng]
                               ,PA.[Id]
                               ,PA.[Description]
                               ,PA.[Value]
                           FROM [dbo].[Persons] PE WITH (NOLOCK)
                               LEFT JOIN [dbo].[Addresses] AD WITH (NOLOCK) ON AD.[PersonId] = PE.[Id]
                               LEFT JOIN [dbo].[PersonalAssets] PA WITH (NOLOCK) ON PA.[PersonId] = PE.[Id]
                           ORDER BY PE.[FullName], PE.[Id], PA.[Id];
                           """;

        // CommandDefinition to be able to pass the cancellationToken.
        CommandDefinition command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        await using DbConnection connection = CreateConnection();

        IList<Dto.Person> persons = await connection.QueryPersonsAsync(command);
        return persons;
    }

    #endregion
}
