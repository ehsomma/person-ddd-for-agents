namespace Records.Shared.Mediator.Abstractions;

/// <summary>
/// Base class for request handlers that produce a result. Concrete handlers only implement the typed
/// <see cref="Handle(TRequest, CancellationToken)"/>; the non-generic
/// <see cref="IRequestHandlerBase{TResponse}"/> needed by <see cref="ISender"/> is implemented here,
/// once, by casting the request.
/// </summary>
/// <typeparam name="TRequest">Request type handled.</typeparam>
/// <typeparam name="TResponse">Result type produced by handling the request.</typeparam>
public abstract class RequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc/>
    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    Task<TResponse> IRequestHandlerBase<TResponse>.Handle(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        Task<TResponse> handleTask = Handle((TRequest)request, cancellationToken);
        return handleTask;
    }
}

/// <summary>
/// Base class for request handlers that produce no result. Concrete handlers only implement the typed
/// <see cref="Handle(TRequest, CancellationToken)"/>; the non-generic <see cref="IRequestHandlerBase"/>
/// needed by <see cref="ISender"/> is implemented here, once, by casting the request.
/// </summary>
/// <typeparam name="TRequest">Request type handled.</typeparam>
#pragma warning disable SA1402
public abstract class RequestHandler<TRequest> : IRequestHandler<TRequest>
    where TRequest : IRequest
{
    /// <inheritdoc/>
    public abstract Task Handle(TRequest request, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    Task IRequestHandlerBase.Handle(IRequest request, CancellationToken cancellationToken)
    {
        Task handleTask = Handle((TRequest)request, cancellationToken);
        return handleTask;
    }
}
#pragma warning restore SA1402
