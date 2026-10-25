using BuildingBlocks.Domain.Events;

namespace BuildingBlocks.Domain.UnitTests.TestData;

/// <summary>
/// A minimal <see cref="DomainEvent"/> registered by the <see cref="TestAggregate"/>.
/// </summary>
internal sealed class TestDomainEvent : DomainEvent
{
    #region Constructor

    /// <summary>Initializes a new instance of the <see cref="TestDomainEvent"/> class.</summary>
    /// <param name="aggregateId">The ID of the aggregate that registers the event.</param>
    /// <param name="name">A name to identify the event in the tests.</param>
    public TestDomainEvent(string aggregateId, string name)
        : base(aggregateId)
    {
        Name = name;
    }

    #endregion

    #region Properties

    /// <summary>Gets the name that identifies the event in the tests.</summary>
    public string Name { get; }

    #endregion
}
