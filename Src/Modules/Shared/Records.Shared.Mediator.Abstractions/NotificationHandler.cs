namespace Records.Shared.Mediator.Abstractions;

/// <summary>
/// Base class for notification handlers. Concrete handlers only implement the typed
/// <see cref="Handle(TNotification, CancellationToken)"/>; the non-generic
/// <see cref="INotificationHandlerBase"/> needed by <see cref="IPublisher"/> is implemented here, once,
/// by casting the notification.
/// </summary>
/// <typeparam name="TNotification">Notification type handled.</typeparam>
public abstract class NotificationHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification
{
    #region Public methods

    /// <inheritdoc/>
    public abstract Task Handle(TNotification notification, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    Task INotificationHandlerBase.Handle(INotification notification, CancellationToken cancellationToken)
    {
        Task handleTask = Handle((TNotification)notification, cancellationToken);
        return handleTask;
    }

    #endregion
}
