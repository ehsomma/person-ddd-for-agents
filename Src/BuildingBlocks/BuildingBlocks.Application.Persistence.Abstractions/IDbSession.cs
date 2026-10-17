using System.Data.Common;

namespace BuildingBlocks.Application.Persistence.Abstractions;

/// <summary>
/// Represents a session in the database/UOW that contains a connection and a transaction.
/// </summary>
/// <remarks>
/// The <see cref="Connection"/> starts closed: <see cref="IUnitOfWork.BeginTransactionAsync(CancellationToken)"/>
/// opens it, and Dapper opens and closes it by itself when a repository reads outside a transaction.
/// </remarks>
public interface IDbSession : IDisposable
{
    #region Properties

    /// <inheritdoc cref="DbConnection"/>
    DbConnection Connection { get; }

    /// <inheritdoc cref="DbTransaction"/>
    DbTransaction? Transaction { get; set; }

    #endregion
}
