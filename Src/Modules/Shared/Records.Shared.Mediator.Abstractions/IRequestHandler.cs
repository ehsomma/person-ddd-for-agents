namespace Records.Shared.Mediator.Abstractions;

/// <summary>
/// Handles a single request type and produces its result.
/// Implementations are resolved by <see cref="ISender"/> via <c>GetRequiredService</c>, so exactly one
/// handler must be registered per <typeparamref name="TRequest"/>/<typeparamref name="TResponse"/> pair.
/// </summary>
/// <remarks>
/// Inherit from <see cref="RequestHandler{TRequest, TResponse}"/> (or from
/// <c>CommandHandler&lt;TCommand, TResponse&gt;</c>/<c>QueryHandler&lt;TQuery, TResponse&gt;</c> in Records.Shared.Application)
/// instead of implementing this interface directly, so the non-generic
/// <see cref="IRequestHandlerBase{TResponse}"/> is implemented for you.
/// </remarks>
/// <typeparam name="TRequest">Request type handled.</typeparam>
/// <typeparam name="TResponse">Result type produced by handling the request.</typeparam>
public interface IRequestHandler<in TRequest, TResponse> : IRequestHandlerBase<TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Asynchronously handles the given request.
    /// </summary>
    /// <param name="request">Request object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation, containing the request result.</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handles a single request type that produces no result.
/// Implementations are resolved by <see cref="ISender"/> via <c>GetRequiredService</c>, so exactly one
/// handler must be registered per <typeparamref name="TRequest"/>.
/// </summary>
/// <remarks>
/// Inherit from <see cref="RequestHandler{TRequest}"/> (or from <c>CommandHandler&lt;TCommand&gt;</c>)
/// instead of implementing this interface directly, so the non-generic
/// <see cref="IRequestHandlerBase"/> is implemented for you.
/// </remarks>
/// <typeparam name="TRequest">Request type handled.</typeparam>
public interface IRequestHandler<in TRequest> : IRequestHandlerBase
    where TRequest : IRequest
{
    /// <summary>
    /// Asynchronously handles the given request.
    /// </summary>
    /// <param name="request">Request object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Handle(TRequest request, CancellationToken cancellationToken = default);
}
