using System.Data.Common;
using BuildingBlocks.Infra.Persistence;
using Dapper;
using Microsoft.Extensions.Configuration;
using Records.Persons.Application.Person.Queries.GetPersonById;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Infra.Queries.Sql.Person.GetPersonById;

/// <summary>
/// Represents the query repository that reads the <see cref="Dto.Person"/> corresponding to the
/// <see cref="GetPersonByIdQuery"/>. Dapper maps the result directly to the DTOs (no data model, no mappers).
/// </summary>
public sealed class GetPersonByIdRepository : QueryRepository<GetPersonByIdQuery, Dto.Person?>
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPersonByIdRepository"/> class.
    /// </summary>
    /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
    public GetPersonByIdRepository(IConfiguration configuration)
        : base(configuration)
    {
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public override async Task<Dto.Person?> GetAsync(GetPersonByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        // The person, its address and its personal assets in a single round trip (one row per personal asset).
        // NOTE: The column order is the one expected by QueryPersonsAsync (it defines the splitOn).
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
                           WHERE PE.[Id] = @id
                           ORDER BY PA.[Id];
                           """;

        // CommandDefinition to be able to pass the cancellationToken.
        CommandDefinition command = new CommandDefinition(
            sql,
            new { id = query.Id },
            cancellationToken: cancellationToken);

        await using DbConnection connection = CreateConnection();

        IList<Dto.Person> persons = await connection.QueryPersonsAsync(command);

        // Not found (null): the caller decides what to do (e.g. GetPersonByIdQueryHandler throws
        // DomainErrors.Person.NotFound).
        Dto.Person? person = persons.SingleOrDefault();

        return person;
    }

    #endregion
}
