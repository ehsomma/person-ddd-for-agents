using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Records.Shared.Application.DependencyInjection;

/// <summary>
/// Extensions methods for dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Public methods

    /// <summary>
    /// Adds MediatR and all domain services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services, params Assembly[] assemblies)
    {
        return services;
    }

    #endregion
}
