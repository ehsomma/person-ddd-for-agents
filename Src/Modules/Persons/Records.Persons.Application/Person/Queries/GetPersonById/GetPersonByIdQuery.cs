using Records.Shared.Cqrs.Abstractions;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersonById;

/// <summary>
/// Represents a query to get a Person corresponding to the specified ID.
/// </summary>
public sealed class GetPersonByIdQuery : IQuery<Dto.Person>
{
    #region Contructor

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPersonByIdQuery"/> class.
    /// </summary>
    /// <param name="id">The ID of the <see cref="Dto.Person"/> to get.</param>
    public GetPersonByIdQuery(Guid id)
    {
        Id = id;
    }

    #endregion

    #region Properties

    /// <summary>The ID of the <see cref="Dto.Person"/> to get.</summary>
    public Guid Id { get; } // Readonly.

    #endregion
}
