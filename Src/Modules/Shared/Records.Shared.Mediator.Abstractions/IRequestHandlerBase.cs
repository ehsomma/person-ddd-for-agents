namespace Records.Shared.Mediator.Abstractions;

/// <summary>
/// Non-generic (in the request) contract used by <see cref="ISender"/> to invoke a handler when it only
/// knows the request as <see cref="IRequest{TResponse}"/> (its concrete type is known only at runtime).
/// </summary>
/// <remarks>
/// Do not implement this interface directly: inherit from <see cref="RequestHandler{TRequest, TResponse}"/>,
/// which implements it by casting the request to <c>TRequest</c> and forwarding to the typed
/// <see cref="IRequestHandler{TRequest, TResponse}.Handle(TRequest, CancellationToken)"/>.
/// </remarks>
/// <typeparam name="TResponse">Result type produced by handling the request.</typeparam>
public interface IRequestHandlerBase<TResponse>
{
    /// <summary>
    /// Asynchronously handles the given request.
    /// </summary>
    /// <param name="request">Request object. Its runtime type must be the handler's request type.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation, containing the request result.</returns>
    Task<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Non-generic contract used by <see cref="ISender"/> to invoke a handler when it only knows the request
/// as <see cref="IRequest"/> (its concrete type is known only at runtime).
/// </summary>
/// <remarks>
/// Do not implement this interface directly: inherit from <see cref="RequestHandler{TRequest}"/>, which
/// implements it by casting the request to <c>TRequest</c> and forwarding to the typed
/// <see cref="IRequestHandler{TRequest}.Handle(TRequest, CancellationToken)"/>.
/// </remarks>
public interface IRequestHandlerBase
{
    /// <summary>
    /// Asynchronously handles the given request.
    /// </summary>
    /// <param name="request">Request object. Its runtime type must be the handler's request type.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Handle(IRequest request, CancellationToken cancellationToken = default);
}
