using BuildingBlocks.Infra.Persistence.Abstractions;
using BuildingBlocks.Infra.Persistence.Mappings.Abstractions.Mappers;
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
