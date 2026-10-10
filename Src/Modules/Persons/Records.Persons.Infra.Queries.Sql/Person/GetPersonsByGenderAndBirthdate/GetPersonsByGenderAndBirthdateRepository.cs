using System.Data.Common;
using BuildingBlocks.Infra.Persistence;
using Dapper;
using Microsoft.Extensions.Configuration;
using Records.Persons.Application.Person.Queries.GetPersonsByGenderAndBirthdate;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Infra.Queries.Sql.Person.GetPersonsByGenderAndBirthdate;

/// <summary>
/// Represents the query repository that reads the <see cref="Dto.Person"/> list corresponding to the
/// <see cref="GetPersonsByGenderAndBirthdateQuery"/>. Dapper maps the result directly to the DTOs (no data model, no mappers).
/// </summary>
public sealed class GetPersonsByGenderAndBirthdateRepository : QueryRepository<GetPersonsByGenderAndBirthdateQuery, IList<Dto.Person>>
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPersonsByGenderAndBirthdateRepository"/> class.
    /// </summary>
    /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
    public GetPersonsByGenderAndBirthdateRepository(IConfiguration configuration)
        : base(configuration)
    {
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public override async Task<IList<Dto.Person>> GetAsync(
        GetPersonsByGenderAndBirthdateQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        // The persons, their addresses and their personal assets in a single round trip (one row per personal asset).
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
                           WHERE PE.[Gender] = @gender
                               AND PE.[Birthdate] BETWEEN @birthdateFrom AND @birthdateTo
                           ORDER BY PE.[FullName], PE.[Id], PA.[Id];
                           """;

        // CommandDefinition to be able to pass the cancellationToken.
        // NOTE: Gender as DbString (IsAnsi) because the column is varchar: a nvarchar parameter (Dapper default
        // for string) would convert the column and prevent the use of an index.
        CommandDefinition command = new CommandDefinition(
            sql,
            new
            {
                gender = new DbString { Value = query.Gender, IsAnsi = true, Length = 10 },
                birthdateFrom = query.BirthdateFrom,
                birthdateTo = query.BirthdateTo,
            },
            cancellationToken: cancellationToken);

        await using DbConnection connection = CreateConnection();

        IList<Dto.Person> persons = await connection.QueryPersonsAsync(command);
        return persons;
    }

    #endregion
}
