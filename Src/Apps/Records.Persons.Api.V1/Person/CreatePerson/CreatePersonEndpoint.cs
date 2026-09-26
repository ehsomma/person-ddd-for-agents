using Microsoft.AspNetCore.Mvc;
using Records.Persons.Application.Person.Commands.CreatePerson;
using Records.Shared.Infra.Http;
using Records.Shared.Mediator.Abstractions;
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
    /// <param name="cancellationToken">Token de cancelación del request.</param>
    /// <response code="200">Persona creada.</response>
    /// <response code="400">Request inválido.</response>
    private async Task<Dto.Person> PostCreatePerson(
        Dto.Person person,
        [FromHeader(Name = "X-App-Key")] string appKey,
        ISender sender,
        CancellationToken cancellationToken)
    {
        CreatePersonCommand command = new(appKey, person);

        Dto.Person createdPerson = await sender.Send(command, cancellationToken);
        return createdPerson;
    }

    #endregion
}
