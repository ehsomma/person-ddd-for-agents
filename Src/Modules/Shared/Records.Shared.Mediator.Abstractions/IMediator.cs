namespace Records.Shared.Mediator.Abstractions;

/// <summary>
/// Sends requests (<see cref="ISender"/>) and publishes notifications (<see cref="IPublisher"/>).
/// </summary>
/// <remarks>
/// Prefer injecting the narrower <see cref="ISender"/> or <see cref="IPublisher"/> when only one of
/// them is needed; all three resolve to the same scoped instance.
/// </remarks>
public interface IMediator : ISender, IPublisher
{
}
