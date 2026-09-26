namespace Records.Shared.Mediator.Abstractions;

/// <summary>
/// Handles a single notification type.
/// Implementations are resolved by <see cref="IPublisher"/> via <c>GetServices</c>, so any number of
/// handlers (including none) can be registered per <typeparamref name="TNotification"/>.
/// </summary>
/// <remarks>
/// Inherit from <see cref="NotificationHandler{TNotification}"/> (or from <c>DomainEventHandler&lt;TEvent&gt;</c>
/// in Records.Shared.Application) instead of implementing this interface directly, so the
/// non-generic <see cref="INotificationHandlerBase"/> is implemented for you.
/// Although <typeparamref name="TNotification"/> is contravariant, the default Microsoft DI container
/// does not resolve variant registrations: a handler for a base notification type (e.g.
/// INotificationHandler&lt;INotification&gt;) is not invoked for derived notifications.
/// </remarks>
/// <typeparam name="TNotification">Notification type handled.</typeparam>
public interface INotificationHandler<in TNotification> : INotificationHandlerBase
    where TNotification : INotification
{
    /// <summary>
    /// Asynchronously handles the given notification.
    /// </summary>
    /// <param name="notification">Notification object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Handle(TNotification notification, CancellationToken cancellationToken = default);
}
