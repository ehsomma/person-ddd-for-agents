using Microsoft.Extensions.DependencyInjection;
using Records.Shared.Cqrs.Abstractions;

namespace Records.Shared.Cqrs;

/// <summary>
/// Resolves the <see cref="ICommandHandler{TCommand, TCommandResponse}"/> registered for the command
/// from the <see cref="IServiceProvider"/> supplied at construction time and delegates to it.
/// </summary>
public class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandDispatcher"/> class.
    /// </summary>
    /// <param name="serviceProvider">
    /// Provider used to resolve command handlers. Must be scoped to the same lifetime as the
    /// handlers themselves (e.g. the current request's <see cref="IServiceProvider"/>), which is
    /// why this dispatcher is registered as Scoped rather than Singleton.
    /// </param>
    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Resolves the single <see cref="ICommandHandler{TCommand, TCommandResponse}"/> registered
    /// for <typeparamref name="TCommand"/>/<typeparamref name="TCommandResponse"/> and dispatches
    /// the command to it.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command being dispatched.</typeparam>
    /// <typeparam name="TCommandResponse">The type of the response returned by the command's handler.</typeparam>
    /// <param name="command">Command object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the dispatch operation, containing the command result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="command"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">No handler is registered for the command.</exception>
    public Task<TCommandResponse> Dispatch<TCommand, TCommandResponse>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TCommandResponse>
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        ICommandHandler<TCommand, TCommandResponse> handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TCommandResponse>>();
        return handler.Handle(command, cancellationToken);
    }

    /// <summary>
    /// Resolves the single <see cref="ICommandHandler{TCommand}"/> registered for
    /// <typeparamref name="TCommand"/> and dispatches the command to it.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command being dispatched, which has no result.</typeparam>
    /// <param name="command">Command object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the dispatch operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="command"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">No handler is registered for the command.</exception>
    public Task Dispatch<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        ICommandHandler<TCommand> handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        return handler.Handle(command, cancellationToken);
    }
}
