using Records.Shared.Mediator.Abstractions;

namespace Records.Shared.Application.Cqrs;

/// <summary>
/// Base class for handlers of a <see cref="IQuery{TResponse}"/>. Exactly one handler must exist per
/// <typeparamref name="TQuery"/>/<typeparamref name="TResponse"/> pair.
/// </summary>
/// <typeparam name="TQuery">Query type handled.</typeparam>
/// <typeparam name="TResponse">Result type produced by handling the query.</typeparam>
public abstract class QueryHandler<TQuery, TResponse> : RequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
