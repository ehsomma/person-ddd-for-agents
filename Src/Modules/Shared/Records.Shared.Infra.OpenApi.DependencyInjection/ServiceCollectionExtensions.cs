using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Records.Shared.Infra.OpenApi.DependencyInjection;

/// <summary>
/// Extensions methods for dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Public methods

    /// <summary>
    /// Configures OpenAPI document generation for the API (title, description, schema fixes).
    /// </summary>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <param name="title">The API title shown in the OpenAPI document.</param>
    /// <param name="version">The API version shown in the OpenAPI document.</param>
    /// <param name="description">The API description shown in the OpenAPI document.</param>
    /// <returns>The same options instance.</returns>
    /// <remarks>
    /// NOTE: Esto configura las options de un <c>AddOpenApi(...)</c> ya registrado, en vez de llamarlo
    /// directamente, porque el source generator de Microsoft.AspNetCore.OpenApi que lee los comentarios
    /// XML (&lt;summary&gt;, &lt;example&gt;, etc.) de los DTOs resuelve esos comentarios según las
    /// ProjectReference del proyecto donde está el call site literal de <c>AddOpenApi</c>. Esta librería
    /// es compartida y no referencia los DTOs de cada módulo, así que si llamara a <c>AddOpenApi</c> acá
    /// los comentarios XML de los DTOs (por ejemplo Records.Persons.Dtos) no se detectarían nunca. Por
    /// eso <c>AddOpenApi("v1", ...)</c> se llama directamente en el Program.cs de cada API, que sí
    /// referencia (transitivamente) sus propios DTOs, y acá solo se configuran las options.
    /// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/openapi-comments#add-xml-documentation-sources.
    /// </remarks>
    public static OpenApiOptions ConfigureOpenApiCustom(
        this OpenApiOptions options,
        string title,
        string version,
        string description)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

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

        return options;
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
#pragma warning disable IDE0072 // Add missing cases
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
#pragma warning restore IDE0072 // Add missing cases
        }
    }

    #endregion
}
