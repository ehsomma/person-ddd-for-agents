using BuildingBlocks.Infra.Http;
using BuildingBlocks.Mediator.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Records.Persons.Application.Person.Commands.UpdatePerson;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Api.V1.Person.UpdatePerson;

/// <summary>
/// Endpoint para el caso de uso `UpdatePerson`.
/// </summary>
internal sealed class UpdatePersonEndpoint : IEndpoint
{
    #region Public methods

    /// <inheritdoc/>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Crea un grupo de rutas para organizar los endpoints relacionados con un path
        // comun y nombre de grupo.
        RouteGroupBuilder routeGroup = app.MapGroup("/persons").WithTags("Persons");

        routeGroup.MapPut("/{id:guid}", PutUpdatePerson)
            .WithName("PutUpdatePerson");
    }

    #endregion

    #region Private methods

    /// <summary>
    /// PUT persons/{id}.
    /// </summary>
    /// <remarks>Actualiza una persona existente y agrega sus personal assets nuevos.</remarks>
    /// <param name="id">ID de la persona a actualizar (tiene que coincidir con el `Id` del body).</param>
    /// <param name="person">Datos de la persona a actualizar.</param>
    /// <param name="appKey" example="MyAppKey">Clave de la aplicación que origina el pedido.</param>
    /// <param name="sender">Injects the mediator sender used to send the command to its handler.</param>
    /// <param name="cancellationToken">Token de cancelación del request.</param>
    /// <response code="200">Persona actualizada.</response>
    /// <response code="400">Request inválido.</response>
    private async Task<Results<Ok<Dto.Person>, ProblemHttpResult>> PutUpdatePerson(
        Guid id,
        Dto.Person person,
        [FromHeader(Name = "X-App-Key")] string appKey,
        ISender sender,
        CancellationToken cancellationToken)
    {
        // El ID de la ruta identifica el recurso: si el del body no coincide, el request es ambiguo.
        if (person.Id != id)
        {
            ProblemHttpResult badRequest = TypedResults.Problem(
                detail: "The person ID in the route doesn't match the one in the body.",
                statusCode: StatusCodes.Status400BadRequest);
            return badRequest;
        }

        UpdatePersonCommand command = new(appKey, person);

        Dto.Person updatedPerson = await sender.Send(command, cancellationToken);

        Ok<Dto.Person> ok = TypedResults.Ok(updatedPerson);
        return ok;
    }

    #endregion
}
