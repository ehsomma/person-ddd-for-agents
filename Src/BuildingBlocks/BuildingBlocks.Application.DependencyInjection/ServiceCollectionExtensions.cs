using System.Reflection;
using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Application.DependencyInjection;

/// <summary>
/// Extension methods to register the shared application services with an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Public methods

    /// <summary>
    /// Registers the shared application services with the DI framework (e.g. <see cref="IDomainEventPublisher"/>),
    /// and scans <paramref name="assemblies"/> registering every domain service (<see cref="IDomainService"/>)
    /// found in them under its own interfaces (e.g. <c>PersonService</c> as <c>IPersonService</c>).
    /// </summary>
    /// <remarks>
    /// <see cref="IDomainEventPublisher"/> depends on the mediator's <c>IPublisher</c>, so <c>AddMediator</c>
    /// must also be called.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for domain services.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Scoped: depende de IPublisher, que es Scoped.
        services.TryAddScoped<IDomainEventPublisher, DomainEventPublisher>();

        // Registers the domain services.
        // NOTA: El `publicOnly: false` permite registrar servicios internos (`internal`); sin el, Scrutor
        // los saltea en silencio y recien falla DI al resolverlos.
        services.Scan(selector =>
        {
            selector.FromAssemblies(assemblies)
                .AddClasses(
                    classes =>
                    {
                        classes
                            ////.Where(type => type.Name.EndsWith("Service"))
                            .AssignableTo<IDomainService>();
                    },
                    publicOnly: false)
                ////.AsSelf()
                // IDomainService es solo un marcador: se registra bajo sus interfaces propias (p.ej.
                // IPersonService) y no bajo IDomainService, que nadie deberia inyectar.
                .AsImplementedInterfaces(serviceType => serviceType != typeof(IDomainService))
                .WithTransientLifetime();
        });

        return services;
    }

    #endregion
}
