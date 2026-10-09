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
        // NOTE: The column order defines the splitOn: Person | Address (from AD.Id) | LatLng (from AD.Lat) |
        // PersonalAsset (from PA.Id).
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

        // Dapper returns null for a split object when its first column is NULL: no address (AD.Id), no
        // coordinates (AD.Lat) or no personal assets (PA.Id).
        IEnumerable<(Dto.Person Person, Dto.PersonalAsset? PersonalAsset)> rows = await connection.QueryAsync<
            Dto.Person,
            Dto.Address?,
            Dto.LatLng?,
            Dto.PersonalAsset?,
            (Dto.Person, Dto.PersonalAsset?)>(
            command,
            (person, address, latLng, personalAsset) =>
            {
                address?.LatLng = latLng;
                person.Address = address;

                return (person, personalAsset);
            },
            splitOn: "Id,Lat,Id");

        List<(Dto.Person Person, Dto.PersonalAsset? PersonalAsset)> rowList = rows.AsList();

        // Not found: the caller decides what to do (e.g. GetPersonByIdQueryHandler throws DomainErrors.Person.NotFound).
        if (rowList.Count == 0)
        {
            return null;
        }

        // Every row has the same person and address, only the personal asset changes.
        Dto.Person person = rowList[0].Person;
        person.PersonalAssets = rowList
            .Select(row => row.PersonalAsset)
            .OfType<Dto.PersonalAsset>() // Discards the null (person without personal assets).
            .ToList();

        return person;
    }

    #endregion
}
