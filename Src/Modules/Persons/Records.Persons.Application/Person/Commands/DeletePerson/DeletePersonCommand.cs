using BuildingBlocks.Application.Cqrs;

namespace Records.Persons.Application.Person.Commands.DeletePerson;

/// <summary>
/// Represents a command to delete an existing person.
/// </summary>
public sealed class DeletePersonCommand : ICommand
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePersonCommand"/> class.
    /// </summary>
    /// <param name="appKey">The appKey (just an example).</param>
    /// <param name="id">The ID of the person to delete.</param>
    public DeletePersonCommand(string appKey, Guid id)
    {
        AppKey = appKey;
        Id = id;
    }

    #endregion

    #region Properties

    /// <summary>The ID of the person to delete.</summary>
    public Guid Id { get; init; }

    /// <summary>The appKey (just an example).</summary>
    public string AppKey { get; init; }

    #endregion
}
