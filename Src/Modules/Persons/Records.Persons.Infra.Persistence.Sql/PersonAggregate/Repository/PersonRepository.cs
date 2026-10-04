using BuildingBlocks.Infra.Persistence.Abstractions;
using BuildingBlocks.Infra.Persistence.Mappings.Abstractions.Mappers;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Configuration;
using Records.Persons.Domain.PersonAggregate.Repositories;
using DataModel = Records.Persons.Infra.Persistence.Sql.PersonAggregate.Models; // Using aliases.
using DomainModel = Records.Persons.Domain.PersonAggregate.Models; // Using aliases.

namespace Records.Persons.Infra.Persistence.Sql.PersonAggregate.Repository;

/// <summary>
/// Represents the persistence operations of the <see cref="DomainModel.Person"/>.
/// </summary>
public class PersonRepository : BuildingBlocks.Infra.Persistence.Repository, IPersonRepository
{
    #region Declarations

    /// <summary>Defines a mapper to map a <see cref="DomainModel.Person"/> to a <see cref="DataModel.Person"/> and vice versa.</summary>
    private readonly IPersistanceMapper<DomainModel.Person, DataModel.Person> _personMapper;

    #endregion

    #region Contructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonRepository"/> class.
    /// </summary>
    /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
    /// <param name="dbSession">Represents a session in the database/UOW that contains a connection and a transaction.</param>
    /// <param name="personMapper">Defines a mapper to map a <see cref="DomainModel.Person"/> to a<see cref="DataModel.Person"/> and vice versa.</param>
    /// <exception cref="ArgumentNullException">When some argument for the constructor parameters is null.</exception>
    public PersonRepository(
        IConfiguration configuration,
        IDbSession dbSession,
        IPersistanceMapper<DomainModel.Person, DataModel.Person> personMapper)
        : base(configuration, dbSession)
    {
        _personMapper = personMapper ?? throw new ArgumentNullException(nameof(personMapper));
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public async Task InsertAsync(DomainModel.Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        DataModel.Person dataPerson = _personMapper.FromDomainToDataModel(person);

        // Dapper.Contrib.
        await _dbSession.Connection.InsertAsync(dataPerson, _dbSession.Transaction);
        await _dbSession.Connection.InsertAsync(dataPerson.Address, _dbSession.Transaction);

        // NOTE: Sequential (not Task.WhenAll): the inserts share the same connection and transaction, and
        // SqlConnection doesn't support concurrent commands (not even with MultipleActiveResultSets).
        foreach (DataModel.PersonalAsset personalAsset in dataPerson.PersonalAssets.NotNull())
        {
            await _dbSession.Connection.InsertAsync(personalAsset, _dbSession.Transaction);
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(DomainModel.Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        DataModel.Person dataPerson = _personMapper.FromDomainToDataModel(person);

        // Dapper.Contrib (by [ExplicitKey]/[Key], the IDs come from the loaded person).
        await _dbSession.Connection.UpdateAsync(dataPerson, _dbSession.Transaction);
        await _dbSession.Connection.UpdateAsync(dataPerson.Address, _dbSession.Transaction);

        // The domain doesn't update or remove the existing personal assets (Person.Update doesn't touch
        // them), it only adds new ones with Person.AddPersonalAsset(): those are the ones without ID yet.
        // NOTE: Sequential (not Task.WhenAll), see InsertAsync.
        foreach (DataModel.PersonalAsset personalAsset in dataPerson.PersonalAssets.NotNull().Where(personalAsset => personalAsset.Id == 0))
        {
            await _dbSession.Connection.InsertAsync(personalAsset, _dbSession.Transaction);
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(DomainModel.Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        // Physical delete of the whole aggregate in a single round trip. The children go first because the
        // FKs (FK_PersonalAssets_Persons, FK_Addresses_Persons) don't have ON DELETE CASCADE.
        const string sql = """
                           DELETE FROM [dbo].[PersonalAssets] WHERE [PersonId] = @id;
                           DELETE FROM [dbo].[Addresses] WHERE [PersonId] = @id;
                           DELETE FROM [dbo].[Persons] WHERE [Id] = @id;
                           """;

        await _dbSession.Connection.ExecuteAsync(
            sql,
            new { id = person.Id },
            _dbSession.Transaction);
    }

    /// <inheritdoc />
    public async Task<DomainModel.Person?> GetByIdAsync(Guid id)
    {
        // The person, its address and its personal assets in a single round trip to the database.
        const string sql = """
                           SELECT [Id]
                               ,[FullName]
                               ,[Email]
                               ,[Phone]
                               ,[Gender]
                               ,[Birthdate]
                               ,[CreatedOnUtc]
                               ,[UpdatedOnUtc]
                           FROM [dbo].[Persons]
                           WHERE [Id] = @id;

                           SELECT [Id]
                               ,[PersonId]
                               ,[StreetLine1]
                               ,[StreetLine2]
                               ,[City]
                               ,[State]
                               ,[Country]
                               ,[Lat]
                               ,[Lng]
                           FROM [dbo].[Addresses]
                           WHERE [PersonId] = @id;

                           SELECT [Id]
                               ,[PersonId]
                               ,[Description]
                               ,[Value]
                           FROM [dbo].[PersonalAssets]
                           WHERE [PersonId] = @id;
                           """;

        await using SqlMapper.GridReader reader = await _dbSession.Connection.QueryMultipleAsync(
            sql,
            new { id },
            _dbSession.Transaction);

        DataModel.Person? dataPerson = await reader.ReadSingleOrDefaultAsync<DataModel.Person>();

        // Not found: the caller decides what to do (e.g. PersonService throws DomainErrors.Person.NotFound).
        if (dataPerson == null)
        {
            return null;
        }

        dataPerson.Address = await reader.ReadSingleOrDefaultAsync<DataModel.Address>();
        dataPerson.PersonalAssets = (await reader.ReadAsync<DataModel.PersonalAsset>()).ToList();

        DomainModel.Person? person = _personMapper.FromDataModelToDomain(dataPerson);
        return person;
    }

    #endregion
}
