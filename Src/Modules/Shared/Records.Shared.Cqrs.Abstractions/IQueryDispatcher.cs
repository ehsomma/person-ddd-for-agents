namespace Records.Shared.Cqrs.Abstractions;

/// <summary>
/// Resolves the single <see cref="IQueryHandler{TQuery, TQueryResponse}"/> registered
/// for a query type and its response type, and dispatches the query to it.
/// </summary>
public interface IQueryDispatcher
{
    /// <summary>
    /// Asynchronously dispatches a query to its handler.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query being dispatched.</typeparam>
    /// <typeparam name="TQueryResponse">The type of the response returned by the query's handler.</typeparam>
    /// <param name="query">Query object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the dispatch operation.</returns>
    Task<TQueryResponse> Dispatch<TQuery, TQueryResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TQueryResponse>;
}
