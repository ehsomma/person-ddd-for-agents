using Records.Shared.Cqrs.Abstractions;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersonById;

/// <summary>
/// Represents the <see cref="GetPersonByIdQuery"/> handler.
/// </summary>
internal sealed class GetPersonByIdQueryHandler : IQueryHandler<GetPersonByIdQuery, Dto.Person>
{
    /// <inheritdoc />
    public Task<Dto.Person> Handle(GetPersonByIdQuery query, CancellationToken cancellation = default)
    {
        return Task.FromResult(new Dto.Person());
    }
}
