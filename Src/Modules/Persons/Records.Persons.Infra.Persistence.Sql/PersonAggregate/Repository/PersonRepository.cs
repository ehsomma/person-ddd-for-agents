using BuildingBlocks.Application.Persistence.Abstractions;
using BuildingBlocks.Infra.Persistence.Mappings.Abstractions.Mappers;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Configuration;
using Records.Persons.Domain.PersonAggregate.Repositories;
using DataModel = Records.Persons.Infra.Persistence.Sql.PersonAggregate.Models; // Using aliases.
using DomainModel = Records.Persons.Domain.PersonAggregate.Models; // Using aliases.

namespace Records.Persons.Infra.Persistence.Sql.PersonAggregate.Repository;

/// <summary>
/// Representa el repositorio (write side) del aggregate <see cref="DomainModel.Person"/>: lo
/// persiste y lo carga completo con Dapper, usando la conexión y la transacción de la
/// <see cref="IDbSession"/>.
/// </summary>
public class PersonRepository : BuildingBlocks.Infra.Persistence.Repository, IPersonRepository
{
    #region Declarations

    /// <summary>El mapper entre el <see cref="DomainModel.Person"/> del dominio y el <see cref="DataModel.Person"/> de la base, en los dos sentidos.</summary>
    private readonly IPersistenceMapper<DomainModel.Person, DataModel.Person> _personMapper;

    #endregion

    #region Constructor

    /// <summary>Inicializa una nueva instancia de <see cref="PersonRepository"/>.</summary>
    /// <param name="configuration">La configuración de la aplicación.</param>
    /// <param name="dbSession">La sesión de base de datos (UoW), con la conexión y la transacción.</param>
    /// <param name="personMapper">El mapper entre el <see cref="DomainModel.Person"/> del dominio y el <see cref="DataModel.Person"/> de la base, en los dos sentidos.</param>
    /// <exception cref="ArgumentNullException">Cuando alguno de los parámetros es <see langword="null"/>.</exception>
    public PersonRepository(
        IConfiguration configuration,
        IDbSession dbSession,
        IPersistenceMapper<DomainModel.Person, DataModel.Person> personMapper)
        : base(configuration, dbSession)
    {
        _personMapper = personMapper ?? throw new ArgumentNullException(nameof(personMapper));
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public async Task InsertAsync(DomainModel.Person person, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(person);

        DataModel.Person dataPerson = _personMapper.FromDomainToDataModel(person);

        // Dapper.Contrib.
        // NOTE: Dapper.Contrib (InsertAsync/UpdateAsync/DeleteAsync) no acepta un
        // CancellationToken, así que se chequea antes de cada llamada: una cancelación frena antes
        // del comando siguiente (el que está en curso no se interrumpe) y la excepción hace
        // rollback de la transacción (ver CommandHandler.ExecuteInTransaction).
        cancellationToken.ThrowIfCancellationRequested();
        await _dbSession.Connection.InsertAsync(dataPerson, _dbSession.Transaction);

        cancellationToken.ThrowIfCancellationRequested();
        await _dbSession.Connection.InsertAsync(dataPerson.Address, _dbSession.Transaction);

        // NOTE: Secuencial (no Task.WhenAll): los inserts comparten la misma conexión y
        // transacción, y SqlConnection no soporta comandos concurrentes (ni siquiera con
        // MultipleActiveResultSets).
        foreach (DataModel.PersonalAsset personalAsset in dataPerson.PersonalAssets.NotNull())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _dbSession.Connection.InsertAsync(personalAsset, _dbSession.Transaction);
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(DomainModel.Person person, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(person);

        DataModel.Person dataPerson = _personMapper.FromDomainToDataModel(person);

        // Dapper.Contrib (por [ExplicitKey]/[Key]; los IDs vienen de la persona cargada).
        // NOTE: Dapper.Contrib no acepta un CancellationToken, se chequea antes de cada llamada
        // (ver InsertAsync).
        cancellationToken.ThrowIfCancellationRequested();
        await _dbSession.Connection.UpdateAsync(dataPerson, _dbSession.Transaction);

        cancellationToken.ThrowIfCancellationRequested();
        await _dbSession.Connection.UpdateAsync(dataPerson.Address, _dbSession.Transaction);

        // El dominio no actualiza ni elimina los bienes personales existentes (Person.Update no los
        // toca), solo agrega nuevos con Person.AddPersonalAsset(): son los que todavía no tienen
        // ID.
        // NOTE: Secuencial (no Task.WhenAll), ver InsertAsync.
        foreach (DataModel.PersonalAsset personalAsset in dataPerson.PersonalAssets.NotNull().Where(personalAsset => personalAsset.Id == 0))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _dbSession.Connection.InsertAsync(personalAsset, _dbSession.Transaction);
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(DomainModel.Person person, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(person);

        // Borrado físico del aggregate completo en un solo round trip. Primero van los hijos porque
        // las FKs (FK_PersonalAssets_Persons, FK_Addresses_Persons) no tienen ON DELETE CASCADE.
        const string sql = """
                           DELETE FROM [dbo].[PersonalAssets] WHERE [PersonId] = @id;
                           DELETE FROM [dbo].[Addresses] WHERE [PersonId] = @id;
                           DELETE FROM [dbo].[Persons] WHERE [Id] = @id;
                           """;

        // Se usa CommandDefinition para poder pasarle el cancellationToken a Dapper.
        CommandDefinition command = new CommandDefinition(
            sql,
            new { id = person.Id },
            _dbSession.Transaction,
            cancellationToken: cancellationToken);

        await _dbSession.Connection.ExecuteAsync(command);
    }

    /// <inheritdoc />
    public async Task<DomainModel.Person?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // La persona, su dirección y sus bienes personales en un solo round trip a la base.
        // NOTE: Sin WITH (NOLOCK) a propósito: carga el aggregate para un command. Si leyera un
        // cambio no confirmado de otra transacción que después hace rollback, el command decidiría
        // y persistiría sobre datos que nunca existieron.
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

        // Se usa CommandDefinition para poder pasarle el cancellationToken a Dapper.
        CommandDefinition command = new CommandDefinition(
            sql,
            new { id },
            _dbSession.Transaction,
            cancellationToken: cancellationToken);

        await using SqlMapper.GridReader reader = await _dbSession.Connection.QueryMultipleAsync(command);

        DataModel.Person? dataPerson = await reader.ReadSingleOrDefaultAsync<DataModel.Person>();

        // Si no existe devuelve null, y el que llama decide qué hacer (p.ej. PersonService lanza
        // DomainErrors.Person.NotFound).
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
