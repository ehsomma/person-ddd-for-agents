using BuildingBlocks.Application.Cqrs;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersons;

/// <summary>
/// Represents a query to get the full list of Persons.
/// </summary>
/// <remarks>
/// It's empty (no filters), but it's still needed: the mediator resolves the handler and the DI resolves
/// the query repository by the type of the query.
/// </remarks>
public sealed class GetPersonsQuery : IQuery<IList<Dto.Person>>
{
}
