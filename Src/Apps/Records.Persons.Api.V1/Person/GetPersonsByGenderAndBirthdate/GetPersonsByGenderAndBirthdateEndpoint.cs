using BuildingBlocks.Infra.Http;
using BuildingBlocks.Mediator.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Records.Persons.Application.Person.Queries.GetPersonsByGenderAndBirthdate;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Api.V1.Person.GetPersonsByGenderAndBirthdate;

/// <summary>
/// Endpoint para el caso de uso `GetPersonsByGenderAndBirthdate` (ejemplo de query con parámetros vía query string).
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

        routeGroup.MapGet("/by-gender-and-birthdate", GetGetPersonsByGenderAndBirthdate)
            .WithName("GetGetPersonsByGenderAndBirthdate");
    }

    #endregion

    #region Private methods

    /// <summary>
    /// GET persons/by-gender-and-birthdate?gender=...&amp;birthDateFrom=...&amp;birthDateTo=....
    /// </summary>
    /// <remarks>Obtiene las personas del género especificado nacidas en el período especificado (inclusive).</remarks>
    /// <param name="gender" example="Male">Género de la persona ("Male, Female, Other").</param>
    /// <param name="birthDateFrom" example="1980-01-01">Fecha de nacimiento desde (inclusive).</param>
    /// <param name="birthDateTo" example="1999-12-31">Fecha de nacimiento hasta (inclusive).</param>
    /// <param name="sender">Injects the mediator sender used to send the query to its handler.</param>
    /// <param name="cancellationToken">Token de cancelación del request.</param>
    /// <response code="200">Lista de personas (vacía si no hay ninguna).</response>
    /// <response code="400">Request inválido (falta algún parámetro o tiene un formato incorrecto).</response>
    private async Task<IList<Dto.Person>> GetGetPersonsByGenderAndBirthdate(
        [FromQuery] string gender,
        [FromQuery] DateTime birthDateFrom,
        [FromQuery] DateTime birthDateTo,
        ISender sender,
        CancellationToken cancellationToken)
    {
        GetPersonsByGenderAndBirthdateQuery query = new GetPersonsByGenderAndBirthdateQuery(gender, birthDateFrom, birthDateTo);

        IList<Dto.Person> persons = await sender.Send(query, cancellationToken);
        return persons;
    }

    #endregion
}
