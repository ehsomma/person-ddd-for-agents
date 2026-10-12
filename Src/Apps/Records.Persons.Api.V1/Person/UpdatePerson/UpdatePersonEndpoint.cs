using BuildingBlocks.Contracts;
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
            .WithName("PutUpdatePerson")
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }

    // NOTE: Es público porque el source generator de Microsoft.AspNetCore.OpenApi solo lee los comentarios
    // XML (<summary>, <remarks>, <param>, <response>) de métodos públicos. Si es privado, Scalar no
    // los muestra.

    /// <summary>
    /// Actualizar persona.
    /// </summary>
    /// <remarks>
    /// Actualiza los datos de una persona existente, le agrega los personal assets nuevos que vengan en el body
    /// y devuelve la persona tal como quedó guardada.
    /// </remarks>
    /// <param name="appKey" example="MobileAppKey">Clave que identifica a la aplicación que origina el request.</param>
    /// <param name="id">ID de la persona a actualizar.</param>
    /// <param name="person">Datos de la persona a actualizar.</param>
    /// <param name="sender">Injects the mediator sender used to send the command to its handler.</param>
    /// <param name="personValidator">Injects the validator of the request body.</param>
    /// <param name="cancellationToken">Token de cancelación del request.</param>
    /// <returns>La persona actualizada.</returns>
    /// <response code="200">Persona actualizada.</response>
    /// <response code="404">La persona no existe.</response>
    public async Task<Dto.Person> PutUpdatePerson(
        [FromHeader(Name = "X-App-Key")] string appKey,
        [FromRoute] Guid id,
        [FromBody] Dto.Person person,
        [FromServices] ISender sender,
        [FromServices] IValidator<Dto.Person> personValidator,
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
