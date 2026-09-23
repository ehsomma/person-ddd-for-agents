using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using My.Data.InterceptableDbConnection;
using Records.Shared.Configuration;
using Records.Shared.Infra.Persistence.Abstractions;

namespace Records.Shared.Infra.Persistence;

/// <inheritdoc cref="IDbSession"/>
public sealed class DbSession : IDbSession
{
    #region Declarations

#pragma warning disable IDE0052 // Remove unread private members
    private readonly Guid _id;
#pragma warning restore IDE0052 // Remove unread private members

    #endregion

    #region Contructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DbSession"/> class.
    /// </summary>
    /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
    public DbSession(IConfiguration configuration)
    {
        _id = Guid.NewGuid();

        SqlServerSettings databaseSettings = configuration.GetSectionOrThrow<SqlServerSettings>(SqlServerSettings.SettingsKey);

        // Connection to the "Source" DataBase.
        ////Connection = new SqlConnection(databaseSettings.SourceConnectionString);
        InterceptedDbConnection connection = new InterceptedDbConnection(new SqlConnection(databaseSettings.SourceConnectionString));

        try
        {
            connection.Open(); // The running proces IP must have access to server.
        }
        catch
        {
            // If Open fails the constructor throws and nobody will call Dispose on this instance.
            connection.Dispose();
            throw;
        }

        Connection = connection;
    }

    #endregion

    #region Prpperties

    /// <inheritdoc />
    public IDbConnection? Connection { get; }

    /// <inheritdoc />
    public IDbTransaction? Transaction { get; set; }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public void Dispose()
    {
        Connection?.Dispose();
    }

    #endregion
}
