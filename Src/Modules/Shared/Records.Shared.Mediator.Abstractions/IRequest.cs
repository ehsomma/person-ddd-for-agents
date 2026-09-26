namespace Records.Shared.Mediator.Abstractions;

/// <summary>
/// Marks a request that produces a <typeparamref name="TResponse"/> when handled.
/// It is sent with <see cref="ISender.Send{TResponse}(IRequest{TResponse}, CancellationToken)"/>, which
/// infers <typeparamref name="TResponse"/> from this interface and resolves the single
/// <see cref="IRequestHandler{TRequest, TResponse}"/> registered for the request's runtime type.
/// </summary>
/// <remarks>
/// Do not implement it directly in application code: use <see cref="ICommand{TResponse}"/> or
/// <see cref="IQuery{TResponse}"/>, which express the CQRS intent.
/// </remarks>
/// <typeparam name="TResponse">Result type produced by handling the request.</typeparam>
public interface IRequest<out TResponse>
{
}

/// <summary>
/// Marks a request that produces no result when handled.
/// It is sent with <see cref="ISender.Send(IRequest, CancellationToken)"/>, which resolves the single
/// <see cref="IRequestHandler{TRequest}"/> registered for the request's runtime type.
/// </summary>
/// <remarks>
/// Do not implement it directly in application code: use <see cref="ICommand"/>, which expresses the
/// CQRS intent.
/// </remarks>
public interface IRequest
{
}
