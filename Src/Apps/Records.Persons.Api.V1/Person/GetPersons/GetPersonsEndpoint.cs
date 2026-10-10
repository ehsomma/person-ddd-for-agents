using BuildingBlocks.Infra.Http;
using BuildingBlocks.Mediator.Abstractions;
using Records.Persons.Application.Person.Queries.GetPersons;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Api.V1.Person.GetPersons;

/// <summary>
/// Endpoint para el caso de uso `GetPersons` (ejemplo de query sin parámetros).
/// </summary>
internal sealed class GetPersonsEndpoint : IEndpoint
{
    #region Public methods

    /// <inheritdoc/>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Crea un grupo de rutas para organizar los endpoints relacionados con un path
        // comun y nombre de grupo.
        RouteGroupBuilder routeGroup = app.MapGroup("/persons").WithTags("Persons");

        routeGroup.MapGet("/", GetGetPersons)
            .WithName("GetGetPersons");
    }

    #endregion

    #region Private methods

    /// <summary>
    /// GET persons.
    /// </summary>
    /// <remarks>Obtiene la lista completa de personas junto con su address y sus personal assets.</remarks>
    /// <param name="sender">Injects the mediator sender used to send the query to its handler.</param>
    /// <param name="cancellationToken">Token de cancelación del request.</param>
    /// <response code="200">Lista de personas (vacía si no hay ninguna).</response>
    private async Task<IList<Dto.Person>> GetGetPersons(
        ISender sender,
        CancellationToken cancellationToken)
    {
        GetPersonsQuery query = new GetPersonsQuery();

        IList<Dto.Person> persons = await sender.Send(query, cancellationToken);
        return persons;
    }

    #endregion
}
