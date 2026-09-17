using Records.Shared.Infra.Http;

namespace Records.Persons.Api.V1.Test;

public class ThrowException1 : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Crea un grupo de rutas para organizar los endpoints relacionados con un path
        // comun y nombre de grupo.
        RouteGroupBuilder routeGroup = app.MapGroup("/tests").WithTags("Tests");

        routeGroup.MapGet("/exception1", ThrowException1)
            .WithName("ThrowException1");
    }

    private IResult ThrowException1(HttpContext context)
    {
#pragma warning disable CA2201
        Exception ex = new Exception("Excepción de prueba 1");
#pragma warning restore CA2201
        ex.Data.Add("TraceIdentifier", context.TraceIdentifier);
        throw ex;
    }
}
