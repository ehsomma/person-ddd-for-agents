using BuildingBlocks.Domain.Events;
using Records.Persons.Domain.PersonAggregate.Models;

namespace Records.Persons.Domain.PersonAggregate.Events;

/// <summary>
/// Representa el evento que indica que se eliminó una persona (<see cref="Models.Person"/>).
/// </summary>
/// <remarks>
/// Lo registra <see cref="Person.Delete"/> y se publica recién después del commit, así que los
/// handlers reaccionan a un borrado que ya está persistido.
/// </remarks>
public sealed class PersonDeletedEvent : DomainEvent
{
    #region Constructor

    /// <summary>Inicializa una nueva instancia de <see cref="PersonDeletedEvent"/>.</summary>
    /// <param name="deletedPerson">La persona eliminada.</param>
    public PersonDeletedEvent(Person deletedPerson)
        : base(deletedPerson.Id.ToString())
    {
        Person = deletedPerson;
    }

    #endregion

    #region Properties

    /// <summary>La persona eliminada.</summary>
    public Person Person { get; private set; }

    #endregion
}
