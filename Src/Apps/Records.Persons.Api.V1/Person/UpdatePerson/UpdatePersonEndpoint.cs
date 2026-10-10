using BuildingBlocks.Infra.Http;
using BuildingBlocks.Mediator.Abstractions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using My.Exceptions;
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
    /// <param name="personValidator">Injects the validator of the request body.</param>
    /// <param name="cancellationToken">Token de cancelación del request.</param>
    /// <response code="200">Persona actualizada.</response>
    /// <response code="400">Request inválido.</response>
    private async Task<Dto.Person> PutUpdatePerson(
        Guid id,
        Dto.Person person,
        [FromHeader(Name = "X-App-Key")] string appKey,
        ISender sender,
        IValidator<Dto.Person> personValidator,
        CancellationToken cancellationToken)
    {
        // El ID de la ruta identifica el recurso: si el del body no coincide, el request es ambiguo. Se lanza
        // como ValidationException para que el GlobalExceptionHandler devuelva el mismo ErrorResponse que el
        // resto de las validaciones.
        if (person.Id != id)
        {
            ValidationError idMismatchError = new ValidationError(
                nameof(Dto.Person.Id),
                "The person ID in the route doesn't match the one in the body.",
                person.Id);

            throw new My.Exceptions.ValidationException(new[] { idMismatchError });
        }

        // Si no es válido lanza una ValidationException con todos los errores (el GlobalExceptionHandler
        // la convierte en un 400 con el detalle en `ValidationErrors`).
        personValidator.ValidateAndThrowExeption(person);

        UpdatePersonCommand command = new UpdatePersonCommand(appKey, person);

        Dto.Person updatedPerson = await sender.Send(command, cancellationToken);

        return updatedPerson;
    }

    #endregion
}
