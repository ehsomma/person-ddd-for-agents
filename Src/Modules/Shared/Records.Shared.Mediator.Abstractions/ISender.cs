namespace Records.Shared.Mediator.Abstractions;

/// <summary>
/// Sends a request to the single handler registered for it (commands and queries).
/// </summary>
public interface ISender
{
    /// <summary>
    /// Asynchronously sends a request to its handler and returns the handler's result.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="TResponse"/> is inferred from <see cref="IRequest{TResponse}"/>, and the handler
    /// is resolved by the request's runtime type (<c>request.GetType()</c>), so
    /// <c>sender.Send(command)</c> needs no explicit generic arguments.
    /// </remarks>
    /// <typeparam name="TResponse">The type of the response returned by the request's handler.</typeparam>
    /// <param name="request">Request object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the send operation, containing the request result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">No handler is registered for the request.</exception>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously sends a request that produces no result to its handler.
    /// </summary>
    /// <remarks>
    /// The handler is resolved by the request's runtime type (<c>request.GetType()</c>).
    /// </remarks>
    /// <param name="request">Request object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the send operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">No handler is registered for the request.</exception>
    Task Send(IRequest request, CancellationToken cancellationToken = default);
}
