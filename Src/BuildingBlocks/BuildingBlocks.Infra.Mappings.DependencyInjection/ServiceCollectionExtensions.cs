using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infra.Mappings.DependencyInjection;

/// <summary>
/// Extension methods to register the mappers.
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Public methods

    /// <summary>
    /// Registers the mappers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddMappers(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.Scan(selector =>
        {
            selector.FromAssemblies(assemblies)
                .AddClasses(classes =>
                {
                    classes.Where(type => type.Name.EndsWith("Mapper", StringComparison.Ordinal));
                })
                .AsImplementedInterfaces()
                .WithTransientLifetime();
        });

        return services;
    }

    #endregion
}
