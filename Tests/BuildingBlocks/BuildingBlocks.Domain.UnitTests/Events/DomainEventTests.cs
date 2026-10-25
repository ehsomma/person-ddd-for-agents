using AwesomeAssertions;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Domain.UnitTests.TestData;
using Xunit;

namespace BuildingBlocks.Domain.UnitTests.Events;

/// <summary>
/// Tests of the <see cref="DomainEvent"/> base class: the aggregate ID and the date when the event
/// occurred.
/// </summary>
public sealed class DomainEventTests
{
    #region Public methods

    [Fact]
    public void Constructor_WhenCalled_ShouldKeepAggregateId()
    {
        // Arrange.
        string aggregateId = Guid.NewGuid().ToString();

        // Act.
        TestDomainEvent domainEvent = new TestDomainEvent(aggregateId, "First");

        // Assert.
        domainEvent.AggregateId.Should().Be(aggregateId);
    }

    [Fact]
    public void Constructor_WhenCalled_ShouldSetEventOccurredAtUtcToNow()
    {
        // Arrange.
        DateTime before = DateTime.UtcNow;

        // Act.
        TestDomainEvent domainEvent = new TestDomainEvent(Guid.NewGuid().ToString(), "First");

        // Assert.
        // La fecha es la de cuando se registró el evento (no la de cuando se publica, que es después
        // del commit): se verifica que caiga entre el antes y el después del Act.
        DateTime after = DateTime.UtcNow;
        domainEvent.EventOccurredAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        domainEvent.EventOccurredAtUtc.Kind.Should().Be(DateTimeKind.Utc);
    }

    #endregion
}
