using Records.Shared.Mediator.Abstractions;

namespace Records.Shared.Application.Cqrs;

/// <summary>
/// Base class for handlers of a <see cref="ICommand{TResponse}"/>. Exactly one handler must exist per
/// <typeparamref name="TCommand"/>/<typeparamref name="TResponse"/> pair.
/// </summary>
/// <typeparam name="TCommand">Command type handled.</typeparam>
/// <typeparam name="TResponse">Result type produced by handling the command.</typeparam>
public abstract class CommandHandler<TCommand, TResponse> : RequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}

/// <summary>
/// Base class for handlers of a <see cref="ICommand"/> that produces no result. Exactly one handler must
/// exist per <typeparamref name="TCommand"/>.
/// </summary>
/// <typeparam name="TCommand">Command type handled.</typeparam>
#pragma warning disable SA1402
public abstract class CommandHandler<TCommand> : RequestHandler<TCommand>
    where TCommand : ICommand
{
}
#pragma warning restore SA1402
