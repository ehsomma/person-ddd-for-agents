using BuildingBlocks.Infra.Http;
using BuildingBlocks.Mediator.Abstractions;
using Microsoft.AspNetCore.Mvc;
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

    // NOTE: El delegado debe ser público para que Scalar tome los comentarios xml.

    /// <summary>
    /// Listar personas.
    /// </summary>
    /// <remarks>
    /// Devuelve todas las personas junto con su address y sus personal assets, sin filtros ni paginado.
    /// </remarks>
    /// <param name="sender">Injects the mediator sender used to send the query to its handler.</param>
    /// <param name="cancellationToken">Token de cancelación del request.</param>
    /// <returns>La lista de personas.</returns>
    /// <response code="200">Lista de personas (vacía si no hay ninguna).</response>
    public async Task<IList<Dto.Person>> GetGetPersons(
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetPersonsQuery query = new GetPersonsQuery();

        IList<Dto.Person> persons = await sender.Send(query, cancellationToken);

        return persons;
    }

    #endregion
}
