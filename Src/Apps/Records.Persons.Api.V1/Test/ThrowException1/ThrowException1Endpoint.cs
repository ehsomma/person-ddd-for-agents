using Records.Shared.Infra.Http;

namespace Records.Persons.Api.V1.Test.ThrowException1;

/// <summary>
/// Endpoint para el caso de uso `ThrowException1`.
/// </summary>
internal sealed class ThrowException1Endpoint : IEndpoint
{
    /// <inheritdoc/>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Crea un grupo de rutas para organizar los endpoints relacionados con un path
        // comun y nombre de grupo.
        RouteGroupBuilder routeGroup = app.MapGroup("/tests").WithTags("Tests");

        routeGroup.MapGet("/exception1", ThrowException1)
            .WithName("ThrowException1");
    }

    /// <summary>
    /// GET test/helloapi/{id}.
    /// </summary>
    /// <remarks>Endpoint de prueba para verificar funcionamiento de Api.</remarks>
    /// <param name="context">HTTP-specific information about an individual HTTP request.</param>
    private IResult ThrowException1(HttpContext context)
    {
#pragma warning disable CA2201
        Exception ex = new Exception("Excepción de prueba 1");
#pragma warning restore CA2201
        ex.Data.Add("TraceIdentifier", context.TraceIdentifier);
        throw ex;
    }
}
