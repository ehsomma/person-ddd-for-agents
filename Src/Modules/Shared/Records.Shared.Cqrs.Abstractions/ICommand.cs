namespace Records.Shared.Cqrs.Abstractions;

/// <summary>
/// Marks a command that produces a <typeparamref name="TCommandResponse"/> when handled.
/// Used only to constrain <see cref="ICommandHandler{TCommand, TCommandResponse}"/> at compile
/// time; it is not used to resolve the handler at runtime (<see cref="ICommandDispatcher"/>
/// still requires both generic arguments explicitly).
/// </summary>
/// <typeparam name="TCommandResponse">Result type produced by handling the command.</typeparam>
public interface ICommand<TCommandResponse>
{
}

/// <summary>
/// Marks a command that produces no result when handled.
/// </summary>
public interface ICommand
{
}
