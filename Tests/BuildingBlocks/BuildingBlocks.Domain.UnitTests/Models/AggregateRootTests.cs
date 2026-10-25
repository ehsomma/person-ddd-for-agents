using AwesomeAssertions;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.UnitTests.TestData;
using Xunit;

namespace BuildingBlocks.Domain.UnitTests.Models;

/// <summary>
/// Tests of the domain events of the <see cref="AggregateRoot{TId}"/> base class: how they are
/// registered and pulled.
/// </summary>
public sealed class AggregateRootTests
{
    #region Public methods

    [Fact]
    public void PullDomainEvents_WhenEventsWereRegistered_ShouldReturnThemInOrder()
    {
        // Arrange.
        // El orden importa: los eventos se publican en el orden en que se registraron (p.ej. primero
        // el PersonUpdatedEvent y después los PersonalAssetAddedEvent).
        TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.DoSomething("First");
        aggregate.DoSomething("Second");

        // Act.
        IReadOnlyCollection<IDomainEvent> domainEvents = aggregate.PullDomainEvents();

        // Assert.
        domainEvents.Cast<TestDomainEvent>().Select(domainEvent => domainEvent.Name)
            .Should().Equal("First", "Second");
    }

    [Fact]
    public void PullDomainEvents_WhenCalledTwice_ShouldReturnEventsOnlyOnce()
    {
        // Arrange.
        // "Pull" saca los eventos: después del primer llamado el aggregate queda sin eventos, así
        // que no se publican dos veces.
        TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.DoSomething("First");
        aggregate.PullDomainEvents();

        // Act.
        IReadOnlyCollection<IDomainEvent> domainEvents = aggregate.PullDomainEvents();

        // Assert.
        domainEvents.Should().BeEmpty();
    }

    [Fact]
    public void PullDomainEvents_WhenCalled_ShouldReturnACopyNotAffectedByLaterEvents()
    {
        // Arrange.
        TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.DoSomething("First");

        // Act.
        IReadOnlyCollection<IDomainEvent> domainEvents = aggregate.PullDomainEvents();
        aggregate.DoSomething("Second");

        // Assert.
        // La colección devuelta es una copia: los eventos que se registran después no aparecen en
        // ella (siguen en el aggregate para el próximo Pull).
        domainEvents.Should().ContainSingle();
    }

    [Fact]
    public void PullDomainEvents_WhenNoEventsWereRegistered_ShouldReturnEmpty()
    {
        // Arrange.
        TestAggregate aggregate = new TestAggregate(Guid.NewGuid());

        // Act.
        IReadOnlyCollection<IDomainEvent> domainEvents = aggregate.PullDomainEvents();

        // Assert.
        domainEvents.Should().BeEmpty();
    }

    #endregion
}
