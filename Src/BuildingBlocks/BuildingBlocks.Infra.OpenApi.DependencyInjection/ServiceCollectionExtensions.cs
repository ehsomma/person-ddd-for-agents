using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BuildingBlocks.Infra.OpenApi.DependencyInjection;

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
    /// Fix para que los ejemplos de entidades (los que se ven en Scalar) respeten el tipo de la propiedad:
    /// que no ponga "True" (con comillas) en lugar de `true` en un bool, ni 1150011234 (sin comillas)
    /// en lugar de "1150011234" en un string.
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

            // NOTE: El generador de comentarios XML deja el <example> en Examples (Example está obsoleto).
            // El tipo del schema lo puso el generador antes que nosotros.
            if (concrete.Examples is null || concrete.Type is not { } type)
            {
                continue;
            }

            for (int i = 0; i < concrete.Examples.Count; i++)
            {
                if (concrete.Examples[i] is not JsonValue value)
                {
                    continue;
                }

                concrete.Examples[i] = ToTypedExample(value, type);
            }
        }
    }

    /// <summary>
    /// Converts the example to the type of the property schema.
    /// </summary>
    /// <param name="value">The example as it was parsed by the XML comments generator.</param>
    /// <param name="type">The type of the property schema (it can include <see cref="JsonSchemaType.Null"/>).</param>
    /// <returns>The example with the type of the property, or the same example if it can not be converted.</returns>
    /// <remarks>
    /// NOTE: El generador parsea el &lt;example&gt; como JSON, así que un texto que es JSON válido
    /// (ej. 1150011234 o true) queda como número o bool aunque la propiedad sea string, y un texto que no
    /// es JSON válido (ej. True) queda como string aunque la propiedad sea bool.
    /// </remarks>
    private static JsonNode ToTypedExample(JsonValue value, JsonSchemaType type)
    {
        JsonNode typedExample = value;

        if (value.TryGetValue(out string? text))
        {
            if (type.HasFlag(JsonSchemaType.Boolean) && bool.TryParse(text, out bool b))
            {
                typedExample = JsonValue.Create(b);
            }
            else if (type.HasFlag(JsonSchemaType.Integer) && long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l))
            {
                typedExample = JsonValue.Create(l);
            }
            else if (type.HasFlag(JsonSchemaType.Number) && decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal d))
            {
                typedExample = JsonValue.Create(d);
            }
        }
        else if (type.HasFlag(JsonSchemaType.String))
        {
            typedExample = JsonValue.Create(value.ToJsonString());
        }

        return typedExample;
    }

    #endregion
}
