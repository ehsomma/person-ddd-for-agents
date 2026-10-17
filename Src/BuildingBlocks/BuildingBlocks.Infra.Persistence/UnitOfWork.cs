using System.Data;
using System.Data.Common;
using BuildingBlocks.Application.Persistence.Abstractions;

namespace BuildingBlocks.Infra.Persistence;

/// <inheritdoc cref="IUnitOfWork"/>
public sealed class UnitOfWork : IUnitOfWork
{
    #region Declarations

    private readonly IDbSession _session;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="session">Represents a session in the database/UOW that contains a connection and a transaction (for projections).</param>
    /// <exception cref="ArgumentNullException">When some argument for the constructor parameters is null.</exception>
    public UnitOfWork(IDbSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // The DbSession creates the connection closed. It may already be open if a previous transaction of the
        // same scope opened it (it stays open until the DbSession is disposed).
        if (_session.Connection.State == ConnectionState.Closed)
        {
            await _session.Connection.OpenAsync(cancellationToken);
        }

        _session.Transaction = await _session.Connection.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CommitAsync()
    {
        DbTransaction? transaction = _session.Transaction;
        if (transaction != null)
        {
            // NOTE: CancellationToken.None on purpose: a commit is never cancelled halfway.
            await transaction.CommitAsync(CancellationToken.None);
        }

        await DisposeTransactionAsync();
    }

    /// <inheritdoc />
    public async Task RollbackAsync()
    {
        DbTransaction? transaction = _session.Transaction;
        if (transaction != null)
        {
            // NOTE: CancellationToken.None on purpose: a rollback is never cancelled halfway.
            await transaction.RollbackAsync(CancellationToken.None);
        }

        await DisposeTransactionAsync();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _session.Transaction?.Dispose();

        // So the repositories used after the commit/rollback don't receive an already disposed transaction.
        _session.Transaction = null;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Disposes the current transaction (if any) and removes it from the <see cref="IDbSession"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task DisposeTransactionAsync()
    {
        DbTransaction? transaction = _session.Transaction;
        if (transaction != null)
        {
            await transaction.DisposeAsync();
        }

        // So the repositories used after the commit/rollback don't receive an already disposed transaction.
        _session.Transaction = null;
    }

    #endregion
}
