using System.Data.Common;
using BuildingBlocks.Infra.Persistence;
using Dapper;
using Microsoft.Extensions.Configuration;
using Records.Persons.Application.Person.Queries.GetPersonById;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Infra.Queries.Sql.Person.GetPersonById;

/// <summary>
/// Representa el query repository que lee la persona (<see cref="Dto.Person"/>) de la
/// <see cref="GetPersonByIdQuery"/>. Dapper mapea el resultado directo a los DTOs, sin data models
/// ni mappers.
/// </summary>
public sealed class GetPersonByIdRepository : QueryRepository<GetPersonByIdQuery, Dto.Person?>
{
    #region Constructor

    /// <summary>Inicializa una nueva instancia de <see cref="GetPersonByIdRepository"/>.</summary>
    /// <param name="configuration">La configuración de la aplicación (de donde sale el connection string).</param>
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

        // La persona, su dirección y sus bienes personales en un solo round trip (una fila por cada
        // bien personal).
        // NOTE: El orden de las columnas es el que espera QueryPersonsAsync (define el splitOn).
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

        // Se usa CommandDefinition para poder pasarle el cancellationToken a Dapper.
        CommandDefinition command = new CommandDefinition(
            sql,
            new { id = query.Id },
            cancellationToken: cancellationToken);

        await using DbConnection connection = CreateConnection();

        IList<Dto.Person> persons = await connection.QueryPersonsAsync(command);

        // Si no existe devuelve null, y el que llama decide qué hacer (p.ej.
        // GetPersonByIdQueryHandler lanza DomainErrors.Person.NotFound).
        Dto.Person? person = persons.SingleOrDefault();

        return person;
    }

    #endregion
}
