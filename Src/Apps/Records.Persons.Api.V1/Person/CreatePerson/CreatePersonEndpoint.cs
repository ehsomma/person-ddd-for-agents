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
            .WithName("PostCreatePerson");
    }

    #endregion

    #region Private methods

    /// <summary>
    /// POST persons.
    /// </summary>
    /// <remarks>Crea una nueva persona.</remarks>
    /// <param name="person">Datos de la persona a crear.</param>
    /// <param name="appKey" example="MyAppKey">Clave de la aplicación que origina el pedido.</param>
    /// <param name="sender">Injects the mediator sender used to send the command to its handler.</param>
    /// <param name="personValidator">Injects the validator of the request body.</param>
    /// <param name="cancellationToken">Token de cancelación del request.</param>
    /// <response code="200">Persona creada.</response>
    /// <response code="400">Request inválido.</response>
    private async Task<Dto.Person> PostCreatePerson(
        Dto.Person person,
        [FromHeader(Name = "X-App-Key")] string appKey,
        ISender sender,
        IValidator<Dto.Person> personValidator,
        CancellationToken cancellationToken)
    {
        // Si no es válido lanza una ValidationException con todos los errores (el GlobalExceptionHandler
        // la convierte en un 400 con el detalle en `ValidationErrors`).
        personValidator.ValidateAndThrowExeption(person);

        CreatePersonCommand command = new CreatePersonCommand(appKey, person);

        Dto.Person createdPerson = await sender.Send(command, cancellationToken);
        return createdPerson;
    }

    #endregion
}
