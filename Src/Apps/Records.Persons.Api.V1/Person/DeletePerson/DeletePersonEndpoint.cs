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
        // Crea un grupo de rutas para organizar los endpoints relacionados con un path
        // comun y nombre de grupo.
        RouteGroupBuilder routeGroup = app.MapGroup("/persons").WithTags("Persons");

        routeGroup.MapDelete("/{id:guid}", DeleteDeletePerson)
            .WithName("DeleteDeletePerson");
    }

    #endregion

    #region Private methods

    /// <summary>
    /// DELETE persons/{id}.
    /// </summary>
    /// <remarks>Elimina una persona existente junto con su address y sus personal assets.</remarks>
    /// <param name="id">ID de la persona a eliminar.</param>
    /// <param name="appKey" example="MyAppKey">Clave de la aplicación que origina el pedido.</param>
    /// <param name="sender">Injects the mediator sender used to send the command to its handler.</param>
    /// <param name="cancellationToken">Token de cancelación del request.</param>
    /// <response code="204">Persona eliminada.</response>
    /// <response code="404">La persona no existe.</response>
    private async Task<NoContent> DeleteDeletePerson(
        Guid id,
        [FromHeader(Name = "X-App-Key")] string appKey,
        ISender sender,
        CancellationToken cancellationToken)
    {
        DeletePersonCommand command = new(appKey, id);

        await sender.Send(command, cancellationToken);

        NoContent noContent = TypedResults.NoContent();
        return noContent;
    }

    #endregion
}
