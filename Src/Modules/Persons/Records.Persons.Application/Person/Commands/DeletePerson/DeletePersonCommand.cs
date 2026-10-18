using BuildingBlocks.Application.Cqrs;

namespace Records.Persons.Application.Person.Commands.DeletePerson;

/// <summary>
/// Representa el command para eliminar una persona existente.
/// </summary>
public sealed class DeletePersonCommand : ICommand
{
    #region Constructor

    /// <summary>Inicializa una nueva instancia de <see cref="DeletePersonCommand"/>.</summary>
    /// <param name="appKey">La app key (es solo un ejemplo).</param>
    /// <param name="id">El ID de la persona a eliminar.</param>
    public DeletePersonCommand(string appKey, Guid id)
    {
        AppKey = appKey;
        Id = id;
    }

    #endregion

    #region Properties

    /// <summary>El ID de la persona a eliminar.</summary>
    public Guid Id { get; init; }

    /// <summary>La app key (es solo un ejemplo).</summary>
    public string AppKey { get; init; }

    #endregion
}
