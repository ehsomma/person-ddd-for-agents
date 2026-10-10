using BuildingBlocks.Infra.Http;
using BuildingBlocks.Mediator.Abstractions;
using Records.Persons.Application.Person.Queries.GetPersonById;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Api.V1.Person.GetPersonById;

/// <summary>
/// Endpoint para el caso de uso `GetPersonById`.
/// </summary>
internal sealed class GetPersonByIdEndpoint : IEndpoint
{
    #region Public methods

    /// <inheritdoc/>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Crea un grupo de rutas para organizar los endpoints relacionados con un path
        // comun y nombre de grupo.
        RouteGroupBuilder routeGroup = app.MapGroup("/persons").WithTags("Persons");

        routeGroup.MapGet("/{id:guid}", GetGetPersonById)
            .WithName("GetGetPersonById");
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Gets the Person corresponding to the specified <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The id to search.</param>
    /// <param name="sender">Injects the mediator sender used to send the query to its handler.</param>
    /// <param name="cancellationToken">Token de cancelación del request.</param>
    /// <response code="200">Persona encontrada.</response>
    /// <response code="404">Si la persona no existe.</response>
    private async Task<Dto.Person> GetGetPersonById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        GetPersonByIdQuery query = new GetPersonByIdQuery(id);

        Dto.Person person = await sender.Send(query, cancellationToken);
        return person;
    }

    #endregion
}
