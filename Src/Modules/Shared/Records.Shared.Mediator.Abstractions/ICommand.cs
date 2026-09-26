namespace Records.Shared.Mediator.Abstractions;

/// <summary>
/// Marks a command (a request that changes state) that produces a <typeparamref name="TResponse"/>
/// when handled by a <see cref="CommandHandler{TCommand, TResponse}"/>.
/// </summary>
/// <typeparam name="TResponse">Result type produced by handling the command.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Marks a command (a request that changes state) that produces no result when handled by a
/// <see cref="CommandHandler{TCommand}"/>.
/// </summary>
public interface ICommand : IRequest
{
}
