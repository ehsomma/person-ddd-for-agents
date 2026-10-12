using BuildingBlocks.Infra.Http;
using Microsoft.AspNetCore.Mvc;

namespace Records.Persons.Api.V1.Test.HelloApi;

/// <summary>
/// Endpoint para el caso de uso `HelloApi`.
/// </summary>
internal sealed class HelloApiEndpoint : IEndpoint
{
    #region Public methods

    /// <inheritdoc/>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Crea un grupo de rutas para organizar los endpoints relacionados con un path
        // comun y nombre de grupo.
        RouteGroupBuilder routeGroup = app.MapGroup("/tests").WithTags("Tests");

        routeGroup.MapGet("/helloapi/{id}", GetTestHelloApi)
            .WithName("GetTestHelloApi");
    }

    // NOTE: Es público porque el source generator de Microsoft.AspNetCore.OpenApi solo lee los comentarios
    // XML (<summary>, <remarks>, <param>, <response>) de métodos públicos. Si es privado, Scalar no
    // los muestra.

    /// <summary>
    /// Probar el funcionamiento del API.
    /// </summary>
    /// <remarks>
    /// Endpoint de prueba: devuelve un mensaje con el ID recibido, para verificar que el API está levantado y
    /// responde. También escribe un warning en el log.
    /// </remarks>
    /// <param name="id" example="Abc123">Texto cualquiera que se devuelve en el mensaje.</param>
    /// <param name="logger">Logger injection.</param>
    /// <param name="httpContext">Datos del contexto HTTP.</param>
    /// <returns>El mensaje de funcionamiento del API.</returns>
    /// <response code="200">Mensaje de funcionamiento ok del API.</response>
    public string GetTestHelloApi(
        [FromRoute] string id,
        [FromServices] ILogger<Program> logger,
        HttpContext httpContext)
    {
        logger.LogWarning("Warning de prueba en GetTestHelloApi() para id {Id}", id);

        return $"Hello Api (check: {id})";
    }

    #endregion
}
