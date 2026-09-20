namespace Records.Shared.Cqrs.Abstractions;

/// <summary>
/// Marks a query that produces a <typeparamref name="TQueryResponse"/> when handled.
/// Used only to constrain <see cref="IQueryHandler{TQuery, TQueryResponse}"/> at compile
/// time; it is not used to resolve the handler at runtime (<see cref="IQueryDispatcher"/>
/// still requires both generic arguments explicitly).
/// </summary>
/// <typeparam name="TQueryResponse">Result type produced by handling the query.</typeparam>
public interface IQuery<out TQueryResponse>
{
}
