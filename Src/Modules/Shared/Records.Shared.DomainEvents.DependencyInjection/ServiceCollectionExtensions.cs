using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Records.Shared.DomainEvents.Abstractions;

namespace Records.Shared.DomainEvents.DependencyInjection;

/// <summary>
/// Extension methods to register the domain event dispatcher and handlers with an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers everything domain events need with the DI framework: <see cref="IDomainEventDispatcher"/>,
    /// and also scans <paramref name="assembly"/> registering every <see cref="IDomainEventHandler{TEvent}"/>
    /// found in it.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan for domain event handlers.</param>
    /// <returns>The same service collection.</returns>
    /// <exception cref="InvalidOperationException">
    /// The same handler class ends up registered more than once for the same event (e.g. the same
    /// assembly was scanned twice).
    /// </exception>
    public static IServiceCollection AddDomainEvents(this IServiceCollection services, Assembly assembly)
    {
        // Scoped: el dispatcher resuelve handlers desde el IServiceProvider que recibe
        // por constructor. Si fuera Singleton, ese IServiceProvider quedaria fijado al
        // root provider y no podria resolver handlers Scoped (p.ej. handlers que dependen
        // de un IUnitOfWork/IDbConnection de Dapper con vida por request).
        services.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // NOTA: El `publicOnly: false` permite registrar handlers internos (`internal`) que es
        // de la forma que los declaramos ya que no se usan desde fuera del ensamblado.
        services.Scan(selector =>
        {
            selector.FromAssemblies(assembly)
                .AddClasses(
                    filter =>
                    {
                        filter.AssignableTo(typeof(IDomainEventHandler<>));
                    },
                    publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime();
        });

        EnsureNoDuplicateHandlerRegistrations(services);

        return services;
    }

    /// <summary>
    /// Unlike commands/queries, an event can legitimately have many handlers, so several registrations
    /// for the same IDomainEventHandler&lt;TEvent&gt; are expected. What is never valid is the
    /// same handler class registered twice for the same event (e.g. <see cref="AddDomainEvents"/>
    /// called twice with the same assembly): the dispatcher would silently run it twice.
    /// This fails fast at startup with a clear message instead of that silent behavior.
    /// </summary>
    private static void EnsureNoDuplicateHandlerRegistrations(IServiceCollection services)
    {
        List<IGrouping<(Type ServiceType, Type? ImplementationType), ServiceDescriptor>> duplicates = services
            .Where(descriptor => descriptor.ServiceType.IsGenericType
                                 && descriptor.ServiceType.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
            .GroupBy(descriptor => (descriptor.ServiceType, descriptor.ImplementationType))
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicates.Count == 0)
        {
            return;
        }

        string details = string.Join("; ", duplicates.Select(group =>
            $"{group.Key.ServiceType} -> {group.Key.ImplementationType?.FullName ?? "unknown"} (x{group.Count()})"));

        throw new InvalidOperationException($"Domain event handler registered more than once for the same event: {details}");
    }
}
