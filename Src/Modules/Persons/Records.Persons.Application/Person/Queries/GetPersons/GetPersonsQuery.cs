using BuildingBlocks.Application.Cqrs;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersons;

/// <summary>
/// Representa una query para obtener la lista completa de Personas.
/// </summary>
/// <remarks>
/// Está vacía (sin filtros), pero aún es necesaria: el mediador resuelve el handler y la DI resuelve
/// el repositorio de la query por el tipo de la query.
/// </remarks>
public sealed class GetPersonsQuery : IQuery<IList<Dto.Person>>
{
}
