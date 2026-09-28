using BuildingBlocks.Domain.Events;
using Records.Persons.Domain.PersonAggregate.Models;

namespace Records.Persons.Domain.PersonAggregate.Events;

/// <summary>
/// Represents an event to indicate that a <see cref="Person"/> was created.
/// </summary>
public sealed class PersonCreatedEvent : DomainEvent
{
    #region Contructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonCreatedEvent"/> class.
    /// </summary>
    /// <param name="createdPerson">The <see cref="Person"/> created.</param>
    public PersonCreatedEvent(Person createdPerson)
        : base(createdPerson.Id.ToString())
    {
        Person = createdPerson;
    }

    #endregion

    #region Properties

    /// <summary>The <see cref="Person"/> created.</summary>
    public Person Person { get; private set; }

    #endregion
}
