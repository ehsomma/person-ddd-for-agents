namespace Records.Shared.Mediator.Abstractions;

/// <summary>
/// Publishes a notification (e.g. a domain event) to every <see cref="INotificationHandler{TNotification}"/>
/// registered for it.
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Asynchronously publishes a notification to all of its handlers.
    /// </summary>
    /// <remarks>
    /// Handlers are resolved by the notification's runtime type (<c>notification.GetType()</c>), not by
    /// the static type of the argument, so notifications can be published from an
    /// <c>IEnumerable&lt;IDomainEvent&gt;</c> without casting. Handlers run sequentially, in
    /// registration order; if none is registered the call completes without doing anything. The first
    /// handler that throws stops the remaining ones and its exception is propagated.
    /// </remarks>
    /// <param name="notification">Notification object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the publish operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="notification"/> is <see langword="null"/>.</exception>
    Task Publish(INotification notification, CancellationToken cancellationToken = default);
}
