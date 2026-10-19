using BuildingBlocks.Application.Cqrs;

namespace Records.Persons.Application.Person.Commands.DeletePerson;

/// <summary>
/// Representa el command para eliminar una persona existente.
/// </summary>
public sealed class DeletePersonCommand : ICommand
{
    #region Constructor

    /// <summary>Inicializa una nueva instancia de <see cref="DeletePersonCommand"/>.</summary>
    /// <param name="id">El ID de la persona a eliminar.</param>
    public DeletePersonCommand(Guid id)
    {
        Id = id;
    }

    #endregion

    #region Properties

    /// <summary>El ID de la persona a eliminar.</summary>
    public Guid Id { get; init; }

    #endregion
}
