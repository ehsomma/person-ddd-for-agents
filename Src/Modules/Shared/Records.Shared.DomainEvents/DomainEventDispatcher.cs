using Microsoft.Extensions.DependencyInjection;
using Records.Shared.DomainEvents.Abstractions;

namespace Records.Shared.DomainEvents;

/// <summary>
/// Resolves every <see cref="IDomainEventHandler{TEvent}"/> registered for the event's runtime type
/// from the <see cref="IServiceProvider"/> supplied at construction time and invokes them sequentially.
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventDispatcher"/> class.
    /// </summary>
    /// <param name="serviceProvider">
    /// Provider used to resolve event handlers. Must be scoped to the same lifetime as the handlers
    /// themselves (e.g. the current request's <see cref="IServiceProvider"/>), which is why this
    /// dispatcher is registered as Scoped rather than Singleton.
    /// </param>
    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public async Task Dispatch(IDomainEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        // El tipo concreto del evento (p.ej. PersonCreated) solo se conoce en runtime, por eso se arma
        // IDomainEventHandler<PersonCreated> con MakeGenericType para pedirle a DI sus handlers.
        Type handlerType = typeof(IDomainEventHandler<>).MakeGenericType(@event.GetType());

        // Todos los IDomainEventHandler<TEvent> heredan de IDomainEventHandler (Non-generic), asi
        // que el cast nunca falla y permite llamar a Handle(IDomainEvent) sin conocer TEvent en compilacion.
        IEnumerable<IDomainEventHandler> handlers = _serviceProvider.GetServices(handlerType).Cast<IDomainEventHandler>();

        foreach (IDomainEventHandler handler in handlers)
        {
            await handler.Handle(@event, cancellationToken);
        }
    }
}
