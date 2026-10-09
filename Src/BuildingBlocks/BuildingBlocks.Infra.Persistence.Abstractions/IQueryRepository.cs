namespace BuildingBlocks.Infra.Persistence.Abstractions;

/// <summary>
/// Defines a query repository: reads from the database the <typeparamref name="TResponse"/> that
/// answers the <typeparamref name="TQuery"/> (one repository per query).
/// </summary>
/// <remarks>
/// The <typeparamref name="TQuery"/> is what identifies the repository in the DI, so it is always required:
/// a query without filters is an empty class (e.g. <c>GetPersonsQuery</c>), never a repository without query
/// (two of them returning the same <typeparamref name="TResponse"/> would be ambiguous).
/// </remarks>
/// <typeparam name="TQuery">The type of the query (the data to use in the filter).</typeparam>
/// <typeparam name="TResponse">The type of the response (usually DTOs).</typeparam>
public interface IQueryRepository<in TQuery, TResponse>
{
    /// <summary>
    /// Gets the <typeparamref name="TResponse"/> that answers the specified <paramref name="query"/>.
    /// </summary>
    /// <param name="query">The query (the data to use in the filter).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The <typeparamref name="TResponse"/> read from the database.</returns>
    Task<TResponse> GetAsync(TQuery query, CancellationToken cancellationToken = default);
}
