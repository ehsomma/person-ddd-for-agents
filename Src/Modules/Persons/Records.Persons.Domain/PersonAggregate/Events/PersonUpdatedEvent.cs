using BuildingBlocks.Domain.Events;
using Records.Persons.Domain.PersonAggregate.Models;

namespace Records.Persons.Domain.PersonAggregate.Events;

/// <summary>
/// Represents an event to indicate that a <see cref="Person"/> was updated.
/// </summary>
public sealed class PersonUpdatedEvent : DomainEvent
{
    #region Contructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonUpdatedEvent"/> class.
    /// </summary>
    /// <param name="updatedPerson">The updated <see cref="Person"/>.</param>
    public PersonUpdatedEvent(Person updatedPerson)
        : base(updatedPerson.Id.ToString())
    {
        Person = updatedPerson;
    }

    #endregion

    #region Properties

    /// <summary>The updated <see cref="Person"/>.</summary>
    public Person Person { get; private set; }

    #endregion
}
