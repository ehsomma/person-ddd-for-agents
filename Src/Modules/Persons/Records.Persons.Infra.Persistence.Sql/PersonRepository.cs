using BuildingBlocks.Infra.Persistence;
using BuildingBlocks.Infra.Persistence.Abstractions;
using Dapper;
using Microsoft.Extensions.Configuration;
using Records.Persons.Domain.PersonAggregate.Repositories;
using DomainModel = Records.Persons.Domain.PersonAggregate.Models; // Using aliases.

namespace Records.Persons.Infra.Persistence.Sql;

/// <summary>
/// Represents the persistence operations of the <see cref="DomainModel.Person"/>.
/// </summary>
public class PersonRepository : Repository, IPersonRepository
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonRepository"/> class.
    /// </summary>
    /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
    /// <param name="dbSession">Represents a session in the database/UOW that contains a connection and a transaction.</param>
    /// <exception cref="ArgumentNullException">When some argument for the constructor parameters is null.</exception>
    public PersonRepository(
        IConfiguration configuration,
        IDbSession dbSession)
        : base(configuration, dbSession)
    {
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public async Task InsertAsync(DomainModel.Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        const string sql = """
                           INSERT INTO [dbo].[Persons]
                               ([Id]
                               ,[FullName]
                               ,[Email]
                               ,[Phone]
                               ,[Gender]
                               ,[Birthdate]
                               ,[CreatedOnUtc]
                               ,[UpdatedOnUtc])
                           VALUES
                               (@id
                               ,@fullName
                               ,@email
                               ,@phone
                               ,@gender
                               ,@birthdate
                               ,@createdOnUtc
                               ,@updatedOnUtc)
                           """;

        await _dbSession.Connection.ExecuteAsync(
            sql,
            new
            {
                id = person.Id,
                fullName = person.FullName,
                email = person.Email,
                phone = person.Phone,
                gender = person.Gender,
                birthdate = person.Birthdate,
                createdOnUtc = person.CreatedOnUtc,
                updatedOnUtc = person.UpdatedOnUtc,
            },
            _dbSession.Transaction);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(DomainModel.Person person)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(DomainModel.Person person)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<DomainModel.Person?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    #endregion
}
