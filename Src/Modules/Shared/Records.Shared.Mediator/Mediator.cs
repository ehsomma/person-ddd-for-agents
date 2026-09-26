using Microsoft.Extensions.DependencyInjection;
using Records.Shared.Mediator.Abstractions;

namespace Records.Shared.Mediator;

/// <summary>
/// Resolves handlers from the <see cref="IServiceProvider"/> supplied at construction time: the single
/// <see cref="IRequestHandler{TRequest, TResponse}"/>/<see cref="IRequestHandler{TRequest}"/> of a request
/// on <c>Send</c>, and every <see cref="INotificationHandler{TNotification}"/> of a notification on
/// <c>Publish</c>.
/// </summary>
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">
    /// Provider used to resolve handlers. Must be scoped to the same lifetime as the handlers
    /// themselves (e.g. the current request's <see cref="IServiceProvider"/>), which is why this
    /// mediator is registered as Scoped rather than Singleton.
    /// </param>
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // El tipo concreto del request (p.ej. CreatePersonCommand) solo se conoce en runtime, por eso se
        // arma IRequestHandler<CreatePersonCommand, TResponse> con MakeGenericType para pedirle a DI su handler.
        Type handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));

        // Todos los IRequestHandler<TRequest, TResponse> heredan de IRequestHandlerBase<TResponse>, asi que
        // el cast nunca falla y permite llamar a Handle(IRequest<TResponse>) sin conocer TRequest en compilacion.
        IRequestHandlerBase<TResponse> handler = (IRequestHandlerBase<TResponse>)_serviceProvider.GetRequiredService(handlerType);

        Task<TResponse> handleTask = handler.Handle(request, cancellationToken);
        return handleTask;
    }

    /// <inheritdoc/>
    public Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Idem Send<TResponse>, para requests sin resultado.
        Type handlerType = typeof(IRequestHandler<>).MakeGenericType(request.GetType());
        IRequestHandlerBase handler = (IRequestHandlerBase)_serviceProvider.GetRequiredService(handlerType);

        Task handleTask = handler.Handle(request, cancellationToken);
        return handleTask;
    }

    /// <inheritdoc/>
    public async Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        // El tipo concreto de la notificacion (p.ej. PersonCreated) solo se conoce en runtime, por eso se arma
        // INotificationHandler<PersonCreated> con MakeGenericType para pedirle a DI sus handlers.
        Type handlerType = typeof(INotificationHandler<>).MakeGenericType(notification.GetType());

        // Todos los INotificationHandler<TNotification> heredan de INotificationHandlerBase, asi que el cast
        // nunca falla y permite llamar a Handle(INotification) sin conocer TNotification en compilacion.
        IEnumerable<INotificationHandlerBase> handlers = _serviceProvider.GetServices(handlerType).Cast<INotificationHandlerBase>();

        // Secuencial (no Task.WhenAll): los handlers comparten dependencias Scoped (IDbConnection/UoW de
        // Dapper) que no son thread-safe.
        foreach (INotificationHandlerBase handler in handlers)
        {
            await handler.Handle(notification, cancellationToken);
        }
    }
}
