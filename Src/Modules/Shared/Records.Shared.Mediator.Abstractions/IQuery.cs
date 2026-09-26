namespace Records.Shared.Mediator.Abstractions;

/// <summary>
/// Marks a query (a request that only reads state) that produces a <typeparamref name="TResponse"/>
/// when handled by a <see cref="QueryHandler{TQuery, TResponse}"/>.
/// </summary>
/// <typeparam name="TResponse">Result type produced by handling the query.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
