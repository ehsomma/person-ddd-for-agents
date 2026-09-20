using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Records.Shared.Cqrs.Abstractions;

namespace Records.Shared.Cqrs.DependencyInjection;

/// <summary>
/// Extension methods to register CQRS dispatchers and handlers with an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers everything CQRS needs with the DI framework: <see cref="ICommandDispatcher"/> and
    /// <see cref="IQueryDispatcher"/>, and also scans <paramref name="assembly"/> registering every
    /// command handler (<see cref="AddCqrsCommandHandlers"/>) and query handler
    /// (<see cref="AddCqrsQueryHandlers"/>) found in it.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan for command and query handlers.</param>
    /// <returns>The same service collection.</returns>
    /// <exception cref="InvalidOperationException">
    /// More than one handler is registered for the same command or query contract.
    /// </exception>
    public static IServiceCollection AddCqrs(this IServiceCollection services, Assembly assembly)
    {
        // Scoped: los dispatchers resuelven handlers desde el IServiceProvider que reciben
        // por constructor. Si fueran Singleton, ese IServiceProvider quedaria fijado al
        // root provider y no podria resolver handlers Scoped (p.ej. handlers que dependen
        // de un IUnitOfWork/IDbConnection de Dapper con vida por request).
        services.TryAddScoped<ICommandDispatcher, CommandDispatcher>();
        services.TryAddScoped<IQueryDispatcher, QueryDispatcher>();

        services.AddCqrsQueryHandlers(assembly);
        services.AddCqrsCommandHandlers(assembly);

        return services;
    }

    /// <summary>
    /// Scans the assembly for classes implementing <see cref="IQueryHandler{TQuery, TQueryResponse}"/>
    /// and registers each one with a Scoped lifetime under its implemented handler interface(s).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan for query handlers.</param>
    /// <returns>The same service collection.</returns>
    /// <exception cref="InvalidOperationException">
    /// More than one handler is registered for the same query contract.
    /// </exception>
    private static IServiceCollection AddCqrsQueryHandlers(this IServiceCollection services, Assembly assembly)
    {
        // NOTA: El `publicOnly: false` permite registrar handlers internos (`internal`) que es
        // de la forma que los declaramos ya que no se usan desde fuera del ensamblado.
        services.Scan(selector =>
        {
            selector.FromAssemblies(assembly)
                .AddClasses(
                    filter =>
                    {
                        filter.AssignableTo(typeof(IQueryHandler<,>));
                    },
                    publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime();
        });

        EnsureSingleHandlerPerContract(services, typeof(IQueryHandler<,>));

        return services;
    }

    /// <summary>
    /// Scans the assembly for classes implementing <see cref="ICommandHandler{TCommand, TCommandResponse}"/>
    /// and registers each one with a Scoped lifetime under its implemented handler interface(s).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan for command handlers.</param>
    /// <returns>The same service collection.</returns>
    /// <exception cref="InvalidOperationException">
    /// More than one handler is registered for the same command contract.
    /// </exception>
    private static IServiceCollection AddCqrsCommandHandlers(this IServiceCollection services, Assembly assembly)
    {
        // NOTA: El `publicOnly: false` permite registrar handlers internos (`internal`) que es
        // de la forma que los declaramos ya que no se usan desde fuera del ensamblado.
        services.Scan(selector =>
        {
            selector.FromAssemblies(assembly)
                .AddClasses(
                    filter =>
                    {
                        filter.AssignableToAny(typeof(ICommandHandler<,>), typeof(ICommandHandler<>));
                    },
                    publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime();
        });

        EnsureSingleHandlerPerContract(services, typeof(ICommandHandler<,>));
        EnsureSingleHandlerPerContract(services, typeof(ICommandHandler<>));

        return services;
    }

    /// <summary>
    /// The dispatchers resolve a single handler per command/query with
    /// <c>GetRequiredService</c>. If two classes end up registered for the same
    /// <paramref name="openHandlerContract"/> (e.g. two <c>IQueryHandler&lt;SameQuery, string&gt;</c>),
    /// the default container silently returns the last one registered instead of failing.
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
            $"{group.Key} -> {string.Join(", ", group.Select(descriptor => descriptor.ImplementationType?.FullName ?? descriptor.ImplementationType?.Name ?? "unknown"))}"));

        throw new InvalidOperationException($"Multiple handlers registered for the same CQRS contract: {details}");
    }
}
