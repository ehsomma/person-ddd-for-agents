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

        await InsertPersonAsync(person);
        await InsertAddressAsync(person.Id, person.Address);
        await InsertPersonalAssetsAsync(person.Id, person.PersonalAssets);
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

    #region Private methods

    /// <summary>
    /// Inserts the <see cref="DomainModel.Person"/> (without its address and personal assets).
    /// </summary>
    /// <param name="person">The <see cref="DomainModel.Person"/> to insert.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task InsertPersonAsync(DomainModel.Person person)
    {
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
                fullName = person.FullName.Value,
                email = person.Email.Value,
                phone = person.Phone?.Value,
                gender = person.Gender.Name,
                birthdate = person.Birthdate,
                createdOnUtc = person.CreatedOnUtc,
                updatedOnUtc = person.UpdatedOnUtc,
            },
            _dbSession.Transaction);
    }

    /// <summary>
    /// Inserts the <see cref="DomainModel.Address"/> of a <see cref="DomainModel.Person"/>.
    /// </summary>
    /// <param name="personId">The ID of the <see cref="DomainModel.Person"/> that the address belongs.</param>
    /// <param name="address">The <see cref="DomainModel.Address"/> to insert.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task InsertAddressAsync(Guid personId, DomainModel.Address address)
    {
        const string sql = """
                           INSERT INTO [dbo].[Addresses]
                               ([PersonId]
                               ,[StreetLine1]
                               ,[StreetLine2]
                               ,[City]
                               ,[State]
                               ,[Country]
                               ,[Lat]
                               ,[Lng])
                           VALUES
                               (@personId
                               ,@streetLine1
                               ,@streetLine2
                               ,@city
                               ,@state
                               ,@country
                               ,@lat
                               ,@lng)
                           """;

        await _dbSession.Connection.ExecuteAsync(
            sql,
            new
            {
                personId,
                streetLine1 = address.StreetLine1.Value,
                streetLine2 = address.StreetLine2?.Value,
                city = address.City?.Value,
                state = address.State?.Value,
                country = address.Country?.Value,
                lat = address.LatLng?.Lat,
                lng = address.LatLng?.Lng,
            },
            _dbSession.Transaction);
    }

    /// <summary>
    /// Inserts the <see cref="DomainModel.PersonalAsset"/> list of a <see cref="DomainModel.Person"/>.
    /// </summary>
    /// <param name="personId">The ID of the <see cref="DomainModel.Person"/> that the personal assets belongs.</param>
    /// <param name="personalAssets">The <see cref="DomainModel.PersonalAsset"/> list to insert.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task InsertPersonalAssetsAsync(Guid personId, IReadOnlyCollection<DomainModel.PersonalAsset> personalAssets)
    {
        const string sql = """
                           INSERT INTO [dbo].[PersonalAssets]
                               ([PersonId]
                               ,[Description]
                               ,[Value])
                           VALUES
                               (@personId
                               ,@description
                               ,@value)
                           """;

        // Dapper executes the command once per item (and nothing if the list is empty).
        await _dbSession.Connection.ExecuteAsync(
            sql,
            personalAssets.Select(personalAsset => new
            {
                personId,
                description = personalAsset.Description.Value,
                value = personalAsset.Value.Value,
            }),
            _dbSession.Transaction);
    }

    #endregion
}
