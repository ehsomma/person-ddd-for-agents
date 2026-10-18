using BuildingBlocks.Application.Cqrs;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Queries.GetPersonById;

/// <summary>
/// Representa la query para obtener la persona (<see cref="Dto.Person"/>) con el ID especificado.
/// </summary>
public sealed class GetPersonByIdQuery : IQuery<Dto.Person>
{
    #region Constructor

    /// <summary>Inicializa una nueva instancia de <see cref="GetPersonByIdQuery"/>.</summary>
    /// <param name="id">El ID de la persona a obtener.</param>
    public GetPersonByIdQuery(Guid id)
    {
        Id = id;
    }

    #endregion

    #region Properties

    /// <summary>El ID de la persona a obtener.</summary>
    public Guid Id { get; } // Solo lectura.

    #endregion
}
