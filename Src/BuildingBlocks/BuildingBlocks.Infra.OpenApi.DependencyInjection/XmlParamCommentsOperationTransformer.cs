using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BuildingBlocks.Infra.OpenApi.DependencyInjection;

/// <summary>
/// TLDR;
/// FIX: (EHS/Claude) 12/10/26 - Corrige un BUG de OpenApi de Microsoft en donde la documentación de
/// los parámetros de los endpoints de minimal APIs queda mal. Si Microsoft corrige el bug, este
/// transformer se puede borrar.
///
///
/// Corrige la documentación de los parámetros de los endpoints de minimal APIs (los &lt;param&gt; del
/// método del endpoint) que deja mal el source generator de Microsoft.AspNetCore.OpenApi.
/// </summary>
/// <remarks>
/// <para>
/// NOTE: BUG del source generator de Microsoft.AspNetCore.OpenApi (XmlCommentOperationTransformer,
/// versión 10.0.x). Para cada &lt;param&gt; del método del endpoint busca un parámetro de la operación
/// (query, header, route) con el mismo nombre que el parámetro de C#. Si no lo encuentra, asume que es
/// el body: pisa la descripción del body con la del &lt;param&gt; y, si tiene <c>example</c>, también
/// el ejemplo del body. Eso pasa con:
/// </para>
/// <list type="bullet">
/// <item>Los servicios inyectados (ISender, IValidator, etc.) y el CancellationToken, que no son parte
/// de la operación. Como gana el último &lt;param&gt;, el body termina mostrando, por ejemplo,
/// "Token de cancelación del request.".</item>
/// <item>Los parámetros con un nombre de binding distinto al de C#, como
/// <c>[FromHeader(Name = "X-Correlation-Id")] string correlationId</c>: en la operación el header se
/// llama <c>X-Correlation-Id</c>, así que no matchea con <c>correlationId</c>. El header queda sin
/// descripción y su ejemplo (p.ej. <c>abc-123</c>, que no es JSON válido) termina como ejemplo del body,
/// y Scalar muestra "Invalid JSON body".</item>
/// </list>
/// <para>
/// El generator registra su transformer antes de ejecutar las options del <c>AddOpenApi</c>, así que
/// este transformer corre después y vuelve a aplicar los &lt;param&gt;, leyéndolos del archivo XML de
/// documentación del assembly del endpoint (requiere <c>GenerateDocumentationFile</c>):
/// </para>
/// <list type="bullet">
/// <item>Body: la descripción y el ejemplo salen solo del &lt;param&gt; del parámetro del body (si no
/// tiene, quedan vacíos).</item>
/// <item>Query/header/route con un nombre de binding distinto al de C#: les aplica la descripción y el
/// ejemplo de su &lt;param&gt;.</item>
/// </list>
/// <para>
/// Supone que el método del endpoint no tiene sobrecargas (busca su documentación por nombre). Si
/// Microsoft corrige el bug, este transformer se puede borrar.
/// </para>
/// </remarks>
internal sealed class XmlParamCommentsOperationTransformer : IOpenApiOperationTransformer
{
    #region Declarations

    private readonly ConcurrentDictionary<Assembly, XDocument?> _xmlDocsByAssembly =
        new ConcurrentDictionary<Assembly, XDocument?>();

    #endregion

    #region Public methods

    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        MethodInfo? method = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<MethodInfo>()
            .SingleOrDefault();

        XElement? methodDoc = method is null ? null : GetMethodDoc(method);

        if (methodDoc is null)
        {
            return Task.CompletedTask;
        }

        foreach (ApiParameterDescription parameterDescription in context.Description.ParameterDescriptions)
        {
            if (parameterDescription.ParameterDescriptor is not IParameterInfoParameterDescriptor
                {
                    ParameterInfo.Name: { } parameterName,
                })
            {
                continue;
            }

            XElement? paramDoc = methodDoc
                .Elements("param")
                .FirstOrDefault(param => (string?)param.Attribute("name") == parameterName);

            string? description = paramDoc is null ? null : NormalizeWhitespace(paramDoc.Value);
            string? example = (string?)paramDoc?.Attribute("example");

            if (parameterDescription.Source == BindingSource.Body)
            {
                ApplyToRequestBody(operation, description, example);
            }
            else if (parameterDescription.Name != parameterName && paramDoc is not null)
            {
                ApplyToParameter(operation, parameterDescription.Name, description, example);
            }
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Sets the description and the example of the request body.
    /// </summary>
    /// <param name="operation">The operation to fix.</param>
    /// <param name="description">The description of the body parameter (null to leave it empty).</param>
    /// <param name="example">The example of the body parameter (null to leave it empty).</param>
    private static void ApplyToRequestBody(OpenApiOperation operation, string? description, string? example)
    {
        if (operation.RequestBody is not OpenApiRequestBody requestBody)
        {
            return;
        }

        requestBody.Description = description;

        if (requestBody.Content is null)
        {
            return;
        }

        // NOTE: Si el ejemplo no es JSON válido se descarta, para que Scalar no muestre "Invalid JSON body".
        JsonNode? exampleNode = TryParseJson(example);

        foreach (OpenApiMediaType mediaType in requestBody.Content.Values.OfType<OpenApiMediaType>())
        {
            mediaType.Example = exampleNode?.DeepClone();
        }
    }

    /// <summary>
    /// Sets the description and the example of a query/header/route parameter of the operation.
    /// </summary>
    /// <param name="operation">The operation to fix.</param>
    /// <param name="name">The name of the parameter in the operation (the binding name, ej. X-Correlation-Id).</param>
    /// <param name="description">The description of the parameter.</param>
    /// <param name="example">The example of the parameter.</param>
    private static void ApplyToParameter(OpenApiOperation operation, string name, string? description, string? example)
    {
        OpenApiParameter? parameter = operation.Parameters?
            .OfType<OpenApiParameter>()
            .FirstOrDefault(p => p.Name == name);

        if (parameter is null)
        {
            return;
        }

        parameter.Description = description;

        if (example is not null)
        {
            // NOTE: En query/header/route un texto que no es JSON (ej. abc-123) es un string.
            parameter.Example = TryParseJson(example) ?? JsonValue.Create(example);
        }
    }

    /// <summary>
    /// Loads the XML documentation file of the assembly (it is next to the dll).
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns>The XML documentation, or null if the file does not exist.</returns>
    private static XDocument? LoadXmlDoc(Assembly assembly)
    {
        string xmlPath = Path.ChangeExtension(assembly.Location, ".xml");

        XDocument? xmlDoc = File.Exists(xmlPath) ? XDocument.Load(xmlPath) : null;

        return xmlDoc;
    }

    /// <summary>
    /// Parses the text as JSON.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <returns>The parsed JSON, or null if the text is null or is not valid JSON.</returns>
    private static JsonNode? TryParseJson(string? text)
    {
        if (text is null)
        {
            return null;
        }

        try
        {
            JsonNode? node = JsonNode.Parse(text);

            return node;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Collapses the line breaks and the indentation of the XML comment into single spaces.
    /// </summary>
    /// <param name="text">The text of the XML comment.</param>
    /// <returns>The text in a single line.</returns>
    private static string NormalizeWhitespace(string text)
    {
        string normalized = string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        return normalized;
    }

    /// <summary>
    /// Gets the XML documentation (the &lt;member&gt; element) of the endpoint method.
    /// </summary>
    /// <param name="method">The method of the endpoint.</param>
    /// <returns>The &lt;member&gt; element, or null if the method has no documentation.</returns>
    private XElement? GetMethodDoc(MethodInfo method)
    {
        if (method.DeclaringType?.FullName is not { } typeName)
        {
            return null;
        }

        XDocument? xmlDoc = _xmlDocsByAssembly.GetOrAdd(method.DeclaringType.Assembly, LoadXmlDoc);

        // Los tipos anidados usan '+' en FullName y '.' en el id de la documentación.
        string memberId = $"M:{typeName.Replace('+', '.')}.{method.Name}";

        XElement? methodDoc = xmlDoc?.Root?
            .Element("members")?
            .Elements("member")
            .FirstOrDefault(member =>
                (string?)member.Attribute("name") is { } name
                && (name == memberId || name.StartsWith(memberId + "(", StringComparison.Ordinal)));

        return methodDoc;
    }

    #endregion
}
