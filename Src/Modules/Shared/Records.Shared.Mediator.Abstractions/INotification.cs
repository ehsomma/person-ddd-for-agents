namespace Records.Shared.Mediator.Abstractions;

/// <summary>
/// Marks a notification that is published with <see cref="IPublisher.Publish(INotification, CancellationToken)"/>
/// to every <see cref="INotificationHandler{TNotification}"/> registered for it (zero or more).
/// </summary>
/// <remarks>
/// Domain events do not implement it (the domain knows nothing about the mediator): the application
/// layer wraps them in <c>DomainMessage&lt;TEvent&gt;</c> (Records.Shared.Application), which does.
/// </remarks>
public interface INotification
{
}
