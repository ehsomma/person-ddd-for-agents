using System.Globalization;
using System.Reflection;
using BuildingBlocks.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infra.Http.DependencyInjection;

/// <summary>
/// Extensions methods for dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Public methods

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

        // Agrega documentación de errores comunes en todos los endpoints.
        RouteGroupBuilder rootGroup = app.MapGroup(string.Empty);
        AddCommonErrorResponses(rootGroup);

        IEnumerable<IEndpoint> endpoints = app.Services.GetServices<IEndpoint>();

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(rootGroup);
        }

        return app;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Documenta en OpenAPI (y por lo tanto en Scalar) las respuestas de error que son iguales en
    /// todos los endpoints de la API (como el 400 y 500), para no tener que declararlas una por una
    /// en cada endpoint.
    /// </summary>
    /// <param name="rootGroup">Grupo dentro del que se mapean todos los endpoints de la API.</param>
    /// <remarks>
    /// <para>
    /// Estos errores no los devuelve el código de cada endpoint sino el GlobalExceptionHandler, siempre
    /// con un <see cref="ErrorResponse"/>, así que son los mismos para toda la API:
    /// </para>
    /// <list type="bullet">
    /// <item><b>500</b> en todos los endpoints: cualquier error inesperado.</item>
    /// <item><b>400</b> solo en los endpoints que reciben un body (los POST, PUT, etc. con <c>[FromBody]</c>):
    /// el body no pasó las validaciones o está mal formado.</item>
    /// </list>
    /// <para>
    /// Si un endpoint necesita otra descripción, alcanza con poner su <c>&lt;response code="..."&gt;</c> en el
    /// comentario XML del método. Si declara su propio 400 (con <c>.Produces(400)</c>), no se agrega el común.
    /// </para>
    /// </remarks>
    private static void AddCommonErrorResponses(RouteGroupBuilder rootGroup)
    {
        rootGroup.WithMetadata(new ProducesResponseTypeAttribute<ErrorResponse>(StatusCodes.Status500InternalServerError)
        {
            Description = "Error inesperado. La respuesta incluye el `logId` para buscar el error en el log.",
        });

        // NOTE: El 400 se agrega en Finally porque recién ahí se sabe si el endpoint recibe un body (minimal APIs
        // lo marca con un IAcceptsMetadata cuando tiene un parámetro [FromBody], después de aplicar el resto de la
        // configuración del grupo). Finally es un default interface method de IEndpointConventionBuilder, por eso
        // se llama a través de la interfaz.
        IEndpointConventionBuilder rootConventions = rootGroup;
        rootConventions.Finally(endpointBuilder =>
        {
            bool hasBody = endpointBuilder.Metadata.OfType<IAcceptsMetadata>().Any();
            bool declaresBadRequest = endpointBuilder.Metadata
                .OfType<IProducesResponseTypeMetadata>()
                .Any(metadata => metadata.StatusCode == StatusCodes.Status400BadRequest);

            if (hasBody && !declaresBadRequest)
            {
                endpointBuilder.Metadata.Add(new ProducesResponseTypeAttribute<ErrorResponse>(StatusCodes.Status400BadRequest)
                {
                    Description = "Request inválido: el body no pasó las validaciones o está mal formado. " +
                                  "Los errores de validación vienen en `validationErrors`.",
                });
            }
        });
    }

    #endregion
}
