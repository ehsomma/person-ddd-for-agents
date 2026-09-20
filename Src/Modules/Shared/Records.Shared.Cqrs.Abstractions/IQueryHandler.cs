namespace Records.Shared.Cqrs.Abstractions;

/// <summary>
/// Handles a single query type and produces its result.
/// Implementations are resolved by <see cref="IQueryDispatcher"/> via <c>GetRequiredService</c>,
/// so exactly one handler must be registered per <typeparamref name="TQuery"/>/<typeparamref name="TQueryResponse"/> pair.
/// </summary>
/// <typeparam name="TQuery">Query type handled.</typeparam>
/// <typeparam name="TQueryResponse">Result type produced by handling the query.</typeparam>
public interface IQueryHandler<in TQuery, TQueryResponse>
    where TQuery : IQuery<TQueryResponse>
{
    /// <summary>
    /// Asynchronously handles the given query.
    /// </summary>
    /// <param name="query">Query object.</param>
    /// <param name="cancellation">Optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation, containing the query result.</returns>
    Task<TQueryResponse> Handle(TQuery query, CancellationToken cancellation = default);
}
