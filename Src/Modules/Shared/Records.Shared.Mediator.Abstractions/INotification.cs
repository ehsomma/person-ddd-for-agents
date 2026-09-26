namespace Records.Shared.Mediator.Abstractions;

/// <summary>
/// Marks a notification that is published with <see cref="IPublisher.Publish(INotification, CancellationToken)"/>
/// to every <see cref="INotificationHandler{TNotification}"/> registered for it (zero or more).
/// </summary>
/// <remarks>
/// Do not implement it directly in application/domain code: use <see cref="IDomainEvent"/>, which
/// expresses the DDD intent.
/// </remarks>
public interface INotification
{
}
