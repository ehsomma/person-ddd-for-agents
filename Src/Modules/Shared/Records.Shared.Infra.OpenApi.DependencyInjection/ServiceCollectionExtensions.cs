#region Usings

using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

#endregion

namespace Records.Shared.Infra.OpenApi.DependencyInjection;

/// <summary>
/// Extensions methods for dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Public methods

    /// <summary>
    /// Configures and registers OpenAPI document generation for the API.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="title">The API title shown in the OpenAPI document.</param>
    /// <param name="version">The API version, used both as the document name and shown in the OpenAPI document.</param>
    /// <param name="description">The API description shown in the OpenAPI document.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddOpenApiCustom(
        this IServiceCollection services,
        string title,
        string version,
        string description)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        // https://localhost:____/openapi/v1.json
        // NOTE: No hace falta agregar archivos XML de otros proyectos, los reconoce automáticamente
        // si el proyecto los genera. Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi(version, options =>
        {
            options.AddDocumentTransformer((doc, _, _) =>
            {
                doc.Info = new OpenApiInfo
                {
                    Title = title,
                    Version = version,
                    Description = description,
                };
                return Task.CompletedTask;
            });

            options.AddSchemaTransformer((schema, _, _) =>
            {
                FixTypedExamples(schema);
                return Task.CompletedTask;
            });
        });

        return services;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Fix para que en los ejemplos de entidades (los que se ven en Scalar) no ponga "True"
    /// (con comillas) en lugar de `true` sin comillas.
    /// </summary>
    /// <param name="schema">The Schema Object allows the definition of input and output data types.</param>
    private static void FixTypedExamples(OpenApiSchema schema)
    {
        if (schema.Properties is null)
        {
            return;
        }

        foreach ((string _, IOpenApiSchema propSchema) in schema.Properties)
        {
            if (propSchema is not OpenApiSchema concrete)
            {
                continue;
            }

            if (concrete.Example is not JsonValue value)
            {
                continue;
            }

            if (!value.TryGetValue(out string? text))
            {
                continue; // solo si quedó como string
            }

            // El tipo del schema lo puso el generador antes que nosotros.
            concrete.Example = concrete.Type switch
            {
                JsonSchemaType.Boolean when bool.TryParse(text, out bool b)
                    => JsonValue.Create(b),
                JsonSchemaType.Integer when long.TryParse(text, out long l)
                    => JsonValue.Create(l),
                JsonSchemaType.Number when decimal.TryParse(text, System.Globalization.CultureInfo.InvariantCulture, out decimal d)
                    => JsonValue.Create(d),
                _ => concrete.Example, // string, guid, fecha: se quedan como están
            };
        }
    }

    #endregion
}
