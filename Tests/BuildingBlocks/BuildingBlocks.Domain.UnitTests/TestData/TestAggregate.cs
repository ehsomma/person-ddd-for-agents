using BuildingBlocks.Domain.Models;

namespace BuildingBlocks.Domain.UnitTests.TestData;

/// <summary>
/// A minimal <see cref="AggregateRoot{TId}"/> with a <see cref="Guid"/> ID, that exposes the
/// protected <c>RegisterDomainEvent</c> through a behavior method, like a real aggregate does.
/// </summary>
internal sealed class TestAggregate : AggregateRoot<Guid>
{
    #region Constructor

    /// <summary>Initializes a new instance of the <see cref="TestAggregate"/> class.</summary>
    /// <param name="id">The aggregate ID.</param>
    public TestAggregate(Guid id)
        : base(id)
    {
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Registers a <see cref="TestDomainEvent"/> with the specified <paramref name="eventName"/>.
    /// </summary>
    /// <param name="eventName">The name that identifies the event in the tests.</param>
    public void DoSomething(string eventName)
    {
        RegisterDomainEvent(new TestDomainEvent(Id.ToString(), eventName));
    }

    #endregion
}
