using BuildingBlocks.Infra.Http;

namespace Records.Persons.Api.V1.Test.ThrowException1;

/// <summary>
/// Endpoint para el caso de uso `ThrowException1`.
/// </summary>
internal sealed class ThrowException1Endpoint : IEndpoint
{
    #region Public methods

    /// <inheritdoc/>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Crea un grupo de rutas para organizar los endpoints relacionados con un path
        // comun y nombre de grupo.
        RouteGroupBuilder routeGroup = app.MapGroup("/tests").WithTags("Tests");

        routeGroup.MapGet("/exception1", ThrowException1)
            .WithName("ThrowException1");
    }

    // NOTE: El delegado debe ser público para que Scalar tome los comentarios xml.

    /// <summary>
    /// Probar un error inesperado.
    /// </summary>
    /// <remarks>
    /// Endpoint de prueba: lanza una excepción no controlada, para probar el manejo de errores
    /// inesperados del API.
    /// </remarks>
    /// <param name="context">HTTP-specific information about an individual HTTP request.</param>
    /// <returns>Nunca devuelve nada: siempre lanza la excepción.</returns>
    /// <response code="500">Error inesperado (el esperado en esta prueba), con el `logId` para buscarlo en el log.</response>
    public IResult ThrowException1(HttpContext context)
    {
#pragma warning disable CA2201
        Exception ex = new Exception("Excepción de prueba 1");
#pragma warning restore CA2201
        ex.Data.Add("TraceIdentifier", context.TraceIdentifier);

        throw ex;
    }

    #endregion
}
