using System.Globalization;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infra.Http.DependencyInjection;

/// <summary>
/// Extensions methods for dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the necessary configurations with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddEndpoints(this IServiceCollection services, params Assembly[] assemblies)
    {
        // NOTE: `publicOnly: false` porque las clases de los endpoints de las APIs se
        // declaran "internal" (CA1515): por defecto Scrutor solo escanea clases públicas
        // y se perderían sin este flag.
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo<IEndpoint>(), publicOnly: false)
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        return services;
    }

    /// <summary>
    /// Registers the FluentValidation validators (<see cref="IValidator{T}"/>) with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddValidators(this IServiceCollection services, params Assembly[] assemblies)
    {
        // NOTE: `publicOnly: false` por lo mismo que en AddEndpoints (los validators de las APIs son
        // "internal"). Se usa Scrutor en vez de FluentValidation.DependencyInjectionExtensions para no
        // sumar otro paquete solo para el scan.
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Fija el idioma de los mensajes en inglés (como el resto de los errores de la API). Si no, FluentValidation
        // usa la cultura del servidor y el mismo request devuelve mensajes en distintos idiomas según dónde corra.
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en");

        return services;
    }

    /// <summary>
    /// Maps the registered endpoints to the application request pipeline.
    /// </summary>
    /// <param name="app">The web application used to configure the HTTP pipeline, and routes.</param>
    /// <returns>The same web application.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="app"/> is null.</exception>
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        IEnumerable<IEndpoint> endpoints = app.Services.GetServices<IEndpoint>();

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}
