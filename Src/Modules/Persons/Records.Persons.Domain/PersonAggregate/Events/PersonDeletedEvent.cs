using BuildingBlocks.Domain.Events;
using Records.Persons.Domain.PersonAggregate.Models;

namespace Records.Persons.Domain.PersonAggregate.Events;

/// <summary>
/// Represents an event to indicate that a <see cref="Person"/> was deleted.
/// </summary>
public sealed class PersonDeletedEvent : DomainEvent
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonDeletedEvent"/> class.
    /// </summary>
    /// <param name="deletedPerson">The <see cref="Person"/> deleted.</param>
    public PersonDeletedEvent(Person deletedPerson)
        : base(deletedPerson.Id.ToString())
    {
        Person = deletedPerson;
    }

    #endregion

    #region Properties

    /// <summary>The <see cref="Person"/> deleted.</summary>
    public Person Person { get; private set; }

    #endregion
}
