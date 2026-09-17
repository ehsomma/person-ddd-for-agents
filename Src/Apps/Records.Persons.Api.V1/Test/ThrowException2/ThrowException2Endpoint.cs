using My.Exceptions;
using Records.Shared.Infra.Http;

namespace Records.Persons.Api.V1.Test.ThrowException2;

/// <summary>
/// Endpoint para el caso de uso `ThrowException2`.
/// </summary>
internal sealed class ThrowException2Endpoint : IEndpoint
{
    /// <inheritdoc/>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Crea un grupo de rutas para organizar los endpoints relacionados con un path
        // comun y nombre de grupo.
        RouteGroupBuilder routeGroup = app.MapGroup("/tests").WithTags("Tests");

        routeGroup.MapGet("/exception2", ThrowException2)
            .WithName("ThrowException2");
    }

    /// <summary>
    /// GET test/exception2.
    /// </summary>
    /// <remarks>Endpoint de prueba para verificar el manejo de excepciones de negocio del Api.</remarks>
    /// <param name="context">HTTP-specific information about an individual HTTP request.</param>
    private IResult ThrowException2(HttpContext context)
    {
        ForbiddenException ex = new ForbiddenException("Excepción de negocio de prueba");
        ex.AddErrorCode("ERR.DOM.TESTEXCEPTION");
        ex.Data.Add("TraceIdentifier", context.TraceIdentifier);
        throw ex;
    }
}
