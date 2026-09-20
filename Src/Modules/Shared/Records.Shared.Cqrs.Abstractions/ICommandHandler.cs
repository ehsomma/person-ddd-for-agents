namespace Records.Shared.Cqrs.Abstractions;

/// <summary>
/// Handles a single command type and produces its result.
/// Implementations are resolved by <see cref="ICommandDispatcher"/> via <c>GetRequiredService</c>,
/// so exactly one handler must be registered per <typeparamref name="TCommand"/>/<typeparamref name="TCommandResponse"/> pair.
/// </summary>
/// <typeparam name="TCommand">Command type handled.</typeparam>
/// <typeparam name="TCommandResponse">Result type produced by handling the command.</typeparam>
public interface ICommandHandler<in TCommand, TCommandResponse>
    where TCommand : ICommand<TCommandResponse>
{
    /// <summary>
    /// Asynchronously handles the given command.
    /// </summary>
    /// <param name="command">Command object.</param>
    /// <param name="cancellation">Optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation, containing the command result.</returns>
    Task<TCommandResponse> Handle(TCommand command, CancellationToken cancellation = default);
}

/// <summary>
/// Handles a single command type that produces no result.
/// Implementations are resolved by <see cref="ICommandDispatcher"/> via <c>GetRequiredService</c>,
/// so exactly one handler must be registered per <typeparamref name="TCommand"/>.
/// </summary>
/// <typeparam name="TCommand">Command type handled.</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Asynchronously handles the given command.
    /// </summary>
    /// <param name="command">Command object.</param>
    /// <param name="cancellation">Optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Handle(TCommand command, CancellationToken cancellation = default);
}
