using BuildingBlocks.Contracts;
using BuildingBlocks.Infra.Http;
using BuildingBlocks.Mediator.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Records.Persons.Application.Person.Queries.GetPersonsByGenderAndBirthdate;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Api.V1.Person.GetPersonsByGenderAndBirthdate;

/// <summary>
/// Endpoint para el caso de uso `GetPersonsByGenderAndBirthdate` (ejemplo de query con parámetros
/// vía query string).
/// </summary>
internal sealed class GetPersonsByGenderAndBirthdateEndpoint : IEndpoint
{
    #region Public methods

    /// <inheritdoc/>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Crea un grupo de rutas para organizar los endpoints relacionados con un path
        // comun y nombre de grupo.
        RouteGroupBuilder routeGroup = app.MapGroup("/persons").WithTags("Persons");

        // El 400 se declara acá porque el común (ver MapEndpoints) solo se agrega en los endpoints con body, y
        // este lo devuelve si falta algún parámetro del query string o tiene un formato incorrecto.
        routeGroup.MapGet("/by-gender-and-birthdate", GetGetPersonsByGenderAndBirthdate)
            .WithName("GetGetPersonsByGenderAndBirthdate")
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);
    }

    // NOTE: El delegado debe ser público para que Scalar tome los comentarios xml.

    /// <summary>
    /// Buscar personas por género y fecha de nacimiento.
    /// </summary>
    /// <remarks>
    /// Devuelve las personas del género especificado nacidas entre las dos fechas especificadas (ambas
    /// inclusive), junto con su address y sus personal assets.
    /// </remarks>
    /// <param name="gender" example="Male">Género de la persona (Male, Female u Other).</param>
    /// <param name="birthDateFrom" example="1980-01-01">Fecha de nacimiento desde (inclusive).</param>
    /// <param name="birthDateTo" example="1999-12-31">Fecha de nacimiento hasta (inclusive).</param>
    /// <param name="sender">Injects the mediator sender used to send the query to its handler.</param>
    /// <param name="cancellationToken">Token de cancelación del request.</param>
    /// <returns>La lista de personas encontradas.</returns>
    /// <response code="200">Lista de personas (vacía si no hay ninguna).</response>
    /// <response code="400">Request inválido: falta algún parámetro o tiene un formato incorrecto.</response>
    public async Task<IList<Dto.Person>> GetGetPersonsByGenderAndBirthdate(
        [FromQuery] string gender,
        [FromQuery] DateTime birthDateFrom,
        [FromQuery] DateTime birthDateTo,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetPersonsByGenderAndBirthdateQuery query = new GetPersonsByGenderAndBirthdateQuery(gender, birthDateFrom, birthDateTo);

        IList<Dto.Person> persons = await sender.Send(query, cancellationToken);

        return persons;
    }

    #endregion
}
