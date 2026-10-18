using BuildingBlocks.Contracts;
using BuildingBlocks.Infra.Http;
using BuildingBlocks.Mediator.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Records.Persons.Application.Person.Commands.DeletePerson;

namespace Records.Persons.Api.V1.Person.DeletePerson;

/// <summary>
/// Endpoint para el caso de uso `DeletePerson`.
/// </summary>
internal sealed class DeletePersonEndpoint : IEndpoint
{
    #region Public methods

    /// <inheritdoc/>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Crea un grupo de rutas para organizar los endpoints relacionados con un path común y
        // nombre de grupo.
        RouteGroupBuilder routeGroup = app.MapGroup("/persons").WithTags("Persons");

        routeGroup.MapDelete("/{id:guid}", DeleteDeletePerson)
            .WithName("DeleteDeletePerson")
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }

    // NOTE: Es público porque el source generator de Microsoft.AspNetCore.OpenApi solo lee los
    // comentarios XML (<summary>, <remarks>, <param>, <response>) de métodos públicos. Si es
    // privado, Scalar no los muestra.

    /// <summary>
    /// Eliminar persona.
    /// </summary>
    /// <remarks>
    /// Elimina una persona existente junto con su address y sus personal assets.
    /// </remarks>
    /// <param name="appKey" example="MobileAppKey">Clave que identifica a la aplicación que origina el request.</param>
    /// <param name="id">ID de la persona a eliminar.</param>
    /// <param name="sender">Sender del mediator, que envía el command a su handler.</param>
    /// <param name="cancellationToken">Token de cancelación del request.</param>
    /// <returns>Una respuesta sin contenido.</returns>
    /// <response code="204">Persona eliminada.</response>
    /// <response code="404">La persona no existe.</response>
    public async Task<NoContent> DeleteDeletePerson(
        [FromHeader(Name = "X-App-Key")] string appKey,
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        DeletePersonCommand command = new DeletePersonCommand(appKey, id);

        await sender.Send(command, cancellationToken);

        NoContent noContent = TypedResults.NoContent();

        return noContent;
    }

    #endregion
}
