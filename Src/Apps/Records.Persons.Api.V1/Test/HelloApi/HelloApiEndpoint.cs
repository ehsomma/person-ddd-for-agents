using Records.Shared.Infra.Http;

namespace Records.Persons.Api.V1.Test;

public class HelloApi : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Crea un grupo de rutas para organizar los endpoints relacionados con un path
        // comun y nombre de grupo.
        RouteGroupBuilder routeGroup = app.MapGroup("/tests").WithTags("Tests");

        routeGroup.MapGet("/helloapi/{id}", GetTestHelloApi)
            .WithName("GetTestHelloApi");
    }

    /// <summary>GET test/helloapi/{id}.</summary>
    /// <remarks>Endpoint de prueba para verificar funcionamiento de Api.</remarks>
    /// <param name="id" example="Abc123">ID único del elemento a recuperar.</param>
    /// <param name="logger">Injects the logger.</param>
    /// <response code="200">Mensaje de funcionamiento ok del Api.</response>
    /// <response code="400">Request inválido.</response>
    private static string GetTestHelloApi(
        string id,
        HttpContext httpContext,
        ILogger<Program> logger)
    {
        logger.LogWarning("Warning de prueba en GetTestHelloApi() para id {Id}", id);

        return $"Hello Api (check: {id})";
    }
}
