using System.Data.Common;
using BuildingBlocks.Application.Persistence.Abstractions;
using BuildingBlocks.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using My.Data.InterceptableDbConnection;

namespace BuildingBlocks.Infra.Persistence;

/// <inheritdoc cref="IDbSession"/>
public sealed class DbSession : IDbSession
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DbSession"/> class.
    /// </summary>
    /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
    public DbSession(IConfiguration configuration)
    {
        SqlServerSettings databaseSettings = configuration.GetSectionOrThrow<SqlServerSettings>(SqlServerSettings.SettingsKey);

        // Connection to the "Source" DataBase.
        // NOTE: Created but not opened (it doesn't go to the server), so a request that fails before using the
        // database doesn't open a connection. UnitOfWork.BeginTransactionAsync opens it, and Dapper opens and
        // closes it by itself for the reads outside a transaction.
        ////Connection = new SqlConnection(databaseSettings.SourceConnectionString);
        Connection = new InterceptedDbConnection(new SqlConnection(databaseSettings.SourceConnectionString));
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public DbConnection Connection { get; }

    /// <inheritdoc />
    public DbTransaction? Transaction { get; set; }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public void Dispose()
    {
        Connection.Dispose();
    }

    #endregion
}
