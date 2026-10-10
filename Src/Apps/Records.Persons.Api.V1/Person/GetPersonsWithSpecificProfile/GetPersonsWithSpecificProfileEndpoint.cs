using BuildingBlocks.Infra.Http;
using BuildingBlocks.Mediator.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Records.Persons.Application.Person.Queries.GetPersonsWithSpecificProfile;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Api.V1.Person.GetPersonsWithSpecificProfile;

/// <summary>
/// Endpoint para el caso de uso `GetPersonsWithSpecificProfile` (ejemplo de query vía POST con el filtro en el body).
/// </summary>
internal sealed class GetPersonsWithSpecificProfileEndpoint : IEndpoint
{
    #region Public methods

    /// <inheritdoc/>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Crea un grupo de rutas para organizar los endpoints relacionados con un path
        // comun y nombre de grupo.
        RouteGroupBuilder routeGroup = app.MapGroup("/persons").WithTags("Persons");

        routeGroup.MapPost("/with-specific-profile", PostGetPersonsWithSpecificProfile)
            .WithName("PostGetPersonsWithSpecificProfile");
    }

    #endregion

    #region Private methods

    /// <summary>
    /// POST persons/with-specific-profile.
    /// </summary>
    /// <remarks>
    /// Obtiene las personas del género especificado, con la edad mínima especificada y que viven en la ciudad
    /// especificada. Es una query (no modifica nada): usa POST solo para poder recibir el filtro en el body.
    /// </remarks>
    /// <param name="query">Filtro de la búsqueda (la query se recibe directamente como body).</param>
    /// <param name="sender">Injects the mediator sender used to send the query to its handler.</param>
    /// <param name="cancellationToken">Token de cancelación del request.</param>
    /// <response code="200">Lista de personas (vacía si no hay ninguna).</response>
    /// <response code="400">Request inválido.</response>
    private async Task<IList<Dto.Person>> PostGetPersonsWithSpecificProfile(
        [FromBody] GetPersonsWithSpecificProfileQuery query,
        ISender sender,
        CancellationToken cancellationToken)
    {
        IList<Dto.Person> persons = await sender.Send(query, cancellationToken);
        return persons;
    }

    #endregion
}
