using BuildingBlocks.Domain.Events;
using Records.Persons.Domain.PersonAggregate.Models;

namespace Records.Persons.Domain.PersonAggregate.Events;

/// <summary>
/// Represents an event to indicate that a <see cref="PersonalAsset"/> was added.
/// </summary>
public sealed class PersonalAssetAddedEvent : DomainEvent
{
    #region Contructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonalAssetAddedEvent"/> class.
    /// </summary>
    /// <param name="person">The <see cref="Person"/> that the <see cref="PersonalAsset"/> belongs.</param>
    /// <param name="newPersonalAsset">The added <see cref="PersonalAsset"/>.</param>
    public PersonalAssetAddedEvent(Person person, PersonalAsset newPersonalAsset)
        : base(newPersonalAsset.Id.ToString())
    {
        Person = person;
        PersonalAsset = newPersonalAsset;
    }

    #endregion

    #region Properties

    /// <summary>The <see cref="Person"/> that the <see cref="PersonalAsset"/> belongs.</summary>
    public Person Person { get; private set; }

    /// <summary>The added <see cref="PersonalAsset"/>.</summary>
    public PersonalAsset PersonalAsset { get; private set; }

    #endregion
}
