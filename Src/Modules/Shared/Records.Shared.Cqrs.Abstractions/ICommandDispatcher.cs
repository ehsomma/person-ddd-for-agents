namespace Records.Shared.Cqrs.Abstractions;

/// <summary>
/// Resolves the single <see cref="ICommandHandler{TCommand, TCommandResponse}"/> registered
/// for a command type and its response type, and dispatches the command to it.
/// </summary>
public interface ICommandDispatcher
{
    /// <summary>
    /// Asynchronously dispatches a command to its handler.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command being dispatched.</typeparam>
    /// <typeparam name="TCommandResponse">The type of the response returned by the command's handler.</typeparam>
    /// <param name="command">Command object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the dispatch operation.</returns>
    Task<TCommandResponse> Dispatch<TCommand, TCommandResponse>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TCommandResponse>;

    /// <summary>
    /// Resolves the single <see cref="ICommandHandler{TCommand}"/> registered for
    /// <typeparamref name="TCommand"/> and dispatches the command to it.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command being dispatched, which has no result.</typeparam>
    /// <param name="command">Command object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the dispatch operation.</returns>
    Task Dispatch<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;
}
