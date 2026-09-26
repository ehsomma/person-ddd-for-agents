using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Records.Shared.Mediator.Abstractions;

namespace Records.Shared.Mediator.DependencyInjection;

/// <summary>
/// Extension methods to register the mediator and its handlers with an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Public methods

    /// <summary>
    /// Registers everything the mediator needs with the DI framework: <see cref="IMediator"/>,
    /// <see cref="ISender"/> and <see cref="IPublisher"/> (the same scoped instance), and every request
    /// handler (commands and queries) and notification handler (domain events) found in
    /// <paramref name="assembly"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan for request and notification handlers.</param>
    /// <returns>The same service collection.</returns>
    /// <exception cref="InvalidOperationException">
    /// More than one handler is registered for the same request contract, or the same notification
    /// handler class ends up registered more than once for the same notification (e.g. the same
    /// assembly was scanned twice).
    /// </exception>
    public static IServiceCollection AddMediator(this IServiceCollection services, Assembly assembly)
    {
        // Scoped: el mediator resuelve handlers desde el IServiceProvider que recibe por constructor.
        // Si fuera Singleton, ese IServiceProvider quedaria fijado al root provider y no podria resolver
        // handlers Scoped (p.ej. handlers que dependen de un IUnitOfWork/IDbConnection de Dapper con vida
        // por request).
        services.TryAddScoped<IMediator, Mediator>();

        // ISender e IPublisher apuntan a la misma instancia Scoped de IMediator.
        services.TryAddScoped<ISender>(serviceProvider => serviceProvider.GetRequiredService<IMediator>());
        services.TryAddScoped<IPublisher>(serviceProvider => serviceProvider.GetRequiredService<IMediator>());

        // NOTA: El `publicOnly: false` permite registrar handlers internos (`internal`) que es
        // de la forma que los declaramos ya que no se usan desde fuera del ensamblado.
        services.Scan(selector =>
        {
            selector.FromAssemblies(assembly)
                .AddClasses(
                    filter =>
                    {
                        filter.AssignableToAny(
                            typeof(IRequestHandler<,>),
                            typeof(IRequestHandler<>),
                            typeof(INotificationHandler<>));
                    },
                    publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime();
        });

        EnsureSingleHandlerPerContract(services, typeof(IRequestHandler<,>));
        EnsureSingleHandlerPerContract(services, typeof(IRequestHandler<>));
        EnsureNoDuplicateNotificationHandlerRegistrations(services);

        return services;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// The mediator resolves a single handler per request with <c>GetRequiredService</c>. If two classes
    /// end up registered for the same <paramref name="openHandlerContract"/> (e.g. two
    /// <c>IRequestHandler&lt;SameQuery, string&gt;</c>), the default container silently returns the last
    /// one registered instead of failing.
    /// This fails fast at startup with a clear message instead of that silent behavior.
    /// </summary>
    private static void EnsureSingleHandlerPerContract(IServiceCollection services, Type openHandlerContract)
    {
        List<IGrouping<Type, ServiceDescriptor>> duplicates = services
            .Where(descriptor => descriptor.ServiceType.IsGenericType
                                 && descriptor.ServiceType.GetGenericTypeDefinition() == openHandlerContract)
            .GroupBy(descriptor => descriptor.ServiceType)
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicates.Count == 0)
        {
            return;
        }

        string details = string.Join("; ", duplicates.Select(group =>
            $"{group.Key} -> {string.Join(", ", group.Select(descriptor => descriptor.ImplementationType?.FullName ?? "unknown"))}"));

        throw new InvalidOperationException($"Multiple handlers registered for the same request contract: {details}");
    }

    /// <summary>
    /// Unlike requests, a notification can legitimately have many handlers, so several registrations
    /// for the same INotificationHandler&lt;TNotification&gt; are expected. What is never valid is the
    /// same handler class registered twice for the same notification (e.g. <see cref="AddMediator"/>
    /// called twice with the same assembly): the mediator would silently run it twice.
    /// This fails fast at startup with a clear message instead of that silent behavior.
    /// </summary>
    private static void EnsureNoDuplicateNotificationHandlerRegistrations(IServiceCollection services)
    {
        List<IGrouping<(Type ServiceType, Type? ImplementationType), ServiceDescriptor>> duplicates = services
            .Where(descriptor => descriptor.ServiceType.IsGenericType
                                 && descriptor.ServiceType.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
            .GroupBy(descriptor => (descriptor.ServiceType, descriptor.ImplementationType))
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicates.Count == 0)
        {
            return;
        }

        string details = string.Join("; ", duplicates.Select(group =>
            $"{group.Key.ServiceType} -> {group.Key.ImplementationType?.FullName ?? "unknown"} (x{group.Count()})"));

        throw new InvalidOperationException($"Notification handler registered more than once for the same notification: {details}");
    }

    #endregion
}
