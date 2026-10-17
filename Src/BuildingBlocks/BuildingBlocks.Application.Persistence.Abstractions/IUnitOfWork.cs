namespace BuildingBlocks.Application.Persistence.Abstractions;

/// <summary>
/// Manage a <see cref="IDbSession"/> to encapsulate a business transaction which can affect the database.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    #region Private methods

    /// <summary>
    /// Begins a database transaction, opening the connection first if it's closed.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token (for opening the connection and beginning the transaction).</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the database transaction.
    /// </summary>
    /// <remarks>
    /// It doesn't receive a <see cref="CancellationToken"/> on purpose: a commit is never cancelled halfway.
    /// </remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CommitAsync();

    /// <summary>
    /// Rolls back a transaction from a pending state.
    /// </summary>
    /// <remarks>
    /// It doesn't receive a <see cref="CancellationToken"/> on purpose: a rollback is never cancelled halfway.
    /// </remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RollbackAsync();

    #endregion
}
