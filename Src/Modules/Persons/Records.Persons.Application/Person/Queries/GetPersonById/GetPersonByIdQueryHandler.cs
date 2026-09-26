using Records.Shared.Mediator.Abstractions;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersonById;

/// <summary>
/// Represents the <see cref="GetPersonByIdQuery"/> handler.
/// </summary>
internal sealed class GetPersonByIdQueryHandler : QueryHandler<GetPersonByIdQuery, Dto.Person>
{
    /// <inheritdoc />
    public override Task<Dto.Person> Handle(GetPersonByIdQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new Dto.Person());
    }
}
