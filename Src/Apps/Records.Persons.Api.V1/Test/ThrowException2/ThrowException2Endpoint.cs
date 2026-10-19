using BuildingBlocks.Contracts;
using BuildingBlocks.Infra.Http;
using My.Exceptions;

namespace Records.Persons.Api.V1.Test.ThrowException2;

/// <summary>
/// Endpoint para el caso de uso `ThrowException2`.
/// </summary>
internal sealed class ThrowException2Endpoint : IEndpoint
{
    #region Public methods

    /// <inheritdoc/>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Crea un grupo de rutas para organizar los endpoints relacionados con un path
        // comun y nombre de grupo.
        RouteGroupBuilder routeGroup = app.MapGroup("/tests").WithTags("Tests");

        routeGroup.MapGet("/exception2", ThrowException2)
            .WithName("ThrowException2")
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);
    }

    // NOTE: El delegado debe ser público para que Scalar tome los comentarios xml.

    /// <summary>
    /// Probar un error de negocio.
    /// </summary>
    /// <remarks>
    /// Endpoint de prueba: lanza una excepción de negocio (ForbiddenException, con su código de error),
    /// para probar el manejo de errores de negocio del API.
    /// </remarks>
    /// <param name="context">HTTP-specific information about an individual HTTP request.</param>
    /// <returns>Nunca devuelve nada: siempre lanza la excepción.</returns>
    /// <response code="403">Error de negocio (el esperado en esta prueba), con su código de error en `code`.</response>
    public IResult ThrowException2(HttpContext context)
    {
        ForbiddenException ex = new ForbiddenException("Excepción de negocio de prueba");
        ex.AddErrorCode("ERR.DOM.TESTEXCEPTION");
        ex.Data.Add("TraceIdentifier", context.TraceIdentifier);

        throw ex;
    }

    #endregion
}
