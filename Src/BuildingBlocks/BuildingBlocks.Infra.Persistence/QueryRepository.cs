using System.Data.Common;
using BuildingBlocks.Configuration;
using BuildingBlocks.Infra.Persistence.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using My.Data.InterceptableDbConnection;

namespace BuildingBlocks.Infra.Persistence;

/// <summary>
/// Represents the base class for all the query repositories: sets the database connection and forces the
/// repository to implement <see cref="GetAsync"/>.
/// </summary>
/// <remarks>
/// The query repositories don't use the <see cref="IDbSession"/> (connection and transaction of the commands):
/// each <see cref="GetAsync"/> creates its own connection with <see cref="CreateConnection"/>, so the reads are
/// independent of the unit of work.
/// </remarks>
/// <typeparam name="TQuery">The type of the query (the data to use in the filter).</typeparam>
/// <typeparam name="TResponse">The type of the response (usually DTOs).</typeparam>
public abstract class QueryRepository<TQuery, TResponse> : IQueryRepository<TQuery, TResponse>
{
    #region Declarations

    /// <summary>The connection string to the database.</summary>
    private readonly string _connectionString;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryRepository{TQuery, TResponse}"/> class.
    /// </summary>
    /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
    /// <exception cref="ArgumentNullException">When some argument for the constructor parameters is null.</exception>
    protected QueryRepository(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        SqlServerSettings databaseSettings = configuration.GetSectionOrThrow<SqlServerSettings>(SqlServerSettings.SettingsKey);
        _connectionString = databaseSettings.SourceConnectionString;
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public abstract Task<TResponse> GetAsync(TQuery query, CancellationToken cancellationToken = default);

    #endregion

    #region Protected methods

    /// <summary>
    /// Creates a new (closed) connection to the database. Dispose it when done (<c>await using</c>); Dapper
    /// opens and closes it automatically if it is closed.
    /// </summary>
    /// <returns>The new connection (intercepted, to log the executed commands).</returns>
    protected DbConnection CreateConnection()
    {
        InterceptedDbConnection connection = new InterceptedDbConnection(new SqlConnection(_connectionString));
        return connection;
    }

    #endregion
}
