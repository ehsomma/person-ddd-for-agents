using BuildingBlocks.Application.Cqrs;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Commands.UpdatePerson;

/// <summary>
/// Represents a command to update an existing person.
/// </summary>
public sealed class UpdatePersonCommand : ICommand<Dto.Person>
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePersonCommand"/> class.
    /// </summary>
    /// <param name="appKey">The appKey (just an example).</param>
    /// <param name="person">The <see cref="Dto.Person"/> object as content data.</param>
    public UpdatePersonCommand(string appKey, Dto.Person person)
    {
        AppKey = appKey;
        Person = person;
    }

    #endregion

    #region Properties

    /// <summary>The <see cref="Dto.Person"/> object as content data.</summary>
    public Dto.Person Person { get; init; }

    /// <summary>The appKey (just an example).</summary>
    public string AppKey { get; init; }

    #endregion
}
