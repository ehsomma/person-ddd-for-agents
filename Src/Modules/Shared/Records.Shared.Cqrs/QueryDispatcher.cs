using Microsoft.Extensions.DependencyInjection;
using Records.Shared.Cqrs.Abstractions;

namespace Records.Shared.Cqrs;

/// <summary>
/// Default <see cref="IQueryDispatcher"/> implementation. Resolves the
/// <see cref="IQueryHandler{TQuery, TQueryResponse}"/> registered for the query from
/// the <see cref="IServiceProvider"/> supplied at construction time and delegates to it.
/// </summary>
public class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryDispatcher"/> class.
    /// </summary>
    /// <param name="serviceProvider">
    /// Provider used to resolve query handlers. Must be scoped to the same lifetime as the
    /// handlers themselves (e.g. the current request's <see cref="IServiceProvider"/>), which is
    /// why this dispatcher is registered as Scoped rather than Singleton.
    /// </param>
    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Resolves the single <see cref="IQueryHandler{TQuery, TQueryResponse}"/> registered
    /// for <typeparamref name="TQuery"/>/<typeparamref name="TQueryResponse"/> and dispatches
    /// the query to it.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query being dispatched.</typeparam>
    /// <typeparam name="TQueryResponse">The type of the response returned by the query's handler.</typeparam>
    /// <param name="query">Query object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the dispatch operation, containing the query result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">No handler is registered for the query.</exception>
    public Task<TQueryResponse> Dispatch<TQuery, TQueryResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TQueryResponse>
    {
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        IQueryHandler<TQuery, TQueryResponse> handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TQueryResponse>>();
        return handler.Handle(query, cancellationToken);
    }
}
