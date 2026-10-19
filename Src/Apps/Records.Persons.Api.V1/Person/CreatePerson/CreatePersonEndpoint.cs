using BuildingBlocks.Infra.Http;
using BuildingBlocks.Mediator.Abstractions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Records.Persons.Application.Person.Commands.CreatePerson;
using Dto = Records.Persons.Dtos.Person;

namespace Records.Persons.Api.V1.Person.CreatePerson;

/// <summary>
/// Endpoint para el caso de uso `CreatePerson`.
/// </summary>
internal sealed class CreatePersonEndpoint : IEndpoint
{
    #region Public methods

    /// <inheritdoc/>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Crea un grupo de rutas para organizar los endpoints relacionados con un path
        // comun y nombre de grupo.
        RouteGroupBuilder routeGroup = app.MapGroup("/persons").WithTags("Persons");

        routeGroup.MapPost("/", PostCreatePerson)
            .WithName("PostCreatePerson")
            .Produces<Dto.Person>(StatusCodes.Status200OK);
    }

    // NOTE: El delegado debe ser público para que Scalar tome los comentarios xml.

    /// <summary>
    /// Crear persona.
    /// </summary>
    /// <remarks>
    /// Crea una persona nueva junto con su address y sus personal assets, y devuelve la persona tal
    /// como quedó
    /// guardada.
    /// </remarks>
    /// <param name="person">Datos de la persona a crear.</param>
    /// <param name="sender">Injects the mediator sender used to send the command to its handler.</param>
    /// <param name="personValidator">Injects the validator of the request body.</param>
    /// <param name="cancellationToken">Token de cancelación del request.</param>
    /// <returns>La persona creada.</returns>
    /// <response code="200">Persona creada.</response>
    public async Task<Dto.Person> PostCreatePerson(
        [FromBody] Dto.Person person,
        [FromServices] ISender sender,
        [FromServices] IValidator<Dto.Person> personValidator,
        CancellationToken cancellationToken)
    {
        // Si no es válido lanza una ValidationException con todos los errores (el GlobalExceptionHandler
        // la convierte en un 400 con el detalle en `ValidationErrors`).
        personValidator.ValidateAndThrowExeption(person);

        CreatePersonCommand command = new CreatePersonCommand(person);

        Dto.Person createdPerson = await sender.Send(command, cancellationToken);

        return createdPerson;
    }

    #endregion
}
