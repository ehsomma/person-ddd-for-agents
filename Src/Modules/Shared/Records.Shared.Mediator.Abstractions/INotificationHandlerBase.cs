namespace Records.Shared.Mediator.Abstractions;

/// <summary>
/// Non-generic contract used by <see cref="IPublisher"/> to invoke a handler when it only knows the
/// notification as <see cref="INotification"/> (its concrete type is known only at runtime).
/// </summary>
/// <remarks>
/// Do not implement this interface directly: inherit from <see cref="NotificationHandler{TNotification}"/>,
/// which implements it by casting the notification to <c>TNotification</c> and forwarding to the typed
/// <see cref="INotificationHandler{TNotification}.Handle(TNotification, CancellationToken)"/>.
/// </remarks>
public interface INotificationHandlerBase
{
    /// <summary>
    /// Asynchronously handles the given notification.
    /// </summary>
    /// <param name="notification">Notification object. Its runtime type must be the handler's notification type.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Handle(INotification notification, CancellationToken cancellationToken = default);
}
