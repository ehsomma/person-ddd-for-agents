using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Records.Shared.Application.DomainEvents;

namespace Records.Shared.Application.DependencyInjection;

/// <summary>
/// Extension methods to register the shared application services with an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the shared application services with the DI framework (e.g. <see cref="IDomainEventPublisher"/>).
    /// </summary>
    /// <remarks>
    /// <see cref="IDomainEventPublisher"/> depends on the mediator's <c>IPublisher</c>, so <c>AddMediator</c>
    /// must also be called.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Scoped: depende de IPublisher, que es Scoped.
        services.TryAddScoped<IDomainEventPublisher, DomainEventPublisher>();

        return services;
    }
}
