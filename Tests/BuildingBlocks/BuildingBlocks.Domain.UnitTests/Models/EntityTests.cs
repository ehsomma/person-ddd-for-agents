using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.UnitTests.TestData;
using Xunit;

namespace BuildingBlocks.Domain.UnitTests.Models;

/// <summary>
/// Tests of the equality of the <see cref="Entity{TId}"/> base class (by ID, not by value; a transient
/// entity, without an ID yet, is only equal to itself).
/// </summary>
public sealed class EntityTests
{
    #region Public methods

    [Fact]
    public void Equals_WhenIdsAreEqual_ShouldBeEqualAndHaveSameHashCode()
    {
        // Arrange.
        // Una entidad se identifica por su ID: dos instancias con el mismo ID son la misma entidad,
        // aunque sean objetos distintos.
        TestEntity entity = new TestEntity(7);
        TestEntity otherEntity = new TestEntity(7);

        // Act.
        bool areEqual = entity == otherEntity;

        // Assert.
        areEqual.Should().BeTrue();
        entity.GetHashCode().Should().Be(otherEntity.GetHashCode());
    }

    [Fact]
    public void Equals_WhenIdsAreDifferent_ShouldNotBeEqual()
    {
        // Arrange.
        TestEntity entity = new TestEntity(7);
        TestEntity otherEntity = new TestEntity(8);

        // Act.
        bool areEqual = entity == otherEntity;

        // Assert.
        areEqual.Should().BeFalse();
    }

    [Fact]
    [SuppressMessage(
        "Maintainability",
        "CA1508:Avoid dead conditional code",
        Justification = "Se testea la sobrecarga del operador == con null: el analyzer asume el == estándar.")]
    public void EqualityOperator_WhenOnlyOneIsNull_ShouldNotBeEqual()
    {
        // Arrange.
        TestEntity entity = new TestEntity(7);
        TestEntity? otherEntity = null;

        // Act.
        bool areEqual = entity == otherEntity;

        // Assert.
        areEqual.Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenTypesAreDifferentWithSameId_ShouldNotBeEqualWithAnyOverload()
    {
        // Arrange.
        // Una dirección y un bien personal con el mismo ID son entidades distintas, y los dos Equals
        // tienen que responder lo mismo: Equals(object) y Equals(Entity<TId>) (el de IEquatable, que
        // es el que usa el operador ==).
        TestEntity entity = new TestEntity(7);
        OtherTestEntity otherEntity = new OtherTestEntity(7);

        // Act.
        bool areEqualAsObjects = entity.Equals((object)otherEntity);
        bool areEqualAsEntities = entity == otherEntity;

        // Assert.
        areEqualAsObjects.Should().BeFalse();
        areEqualAsEntities.Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenBothAreTransient_ShouldNotBeEqualWithAnyOverload()
    {
        // Arrange.
        // Las entidades hijas nuevas tienen ID 0 hasta que se guardan: dos entidades nuevas distintas
        // (dos bienes personales que se agregan juntos) no son la misma entidad aunque compartan el
        // ID 0.
        TestEntity entity = new TestEntity(0);
        TestEntity otherEntity = new TestEntity(0);

        // Act.
        bool areEqualAsObjects = entity.Equals((object)otherEntity);
        bool areEqualAsEntities = entity == otherEntity;

        // Assert.
        areEqualAsObjects.Should().BeFalse();
        areEqualAsEntities.Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenTransientIsComparedWithItself_ShouldBeEqual()
    {
        // Arrange.
        TestEntity entity = new TestEntity(0);
        TestEntity sameEntity = entity;

        // Act.
        bool areEqual = entity == sameEntity;

        // Assert.
        areEqual.Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WhenTwoTransientEntitiesAreAddedToAHashSet_ShouldKeepBoth()
    {
        // Arrange.
        // El hash de una entidad transient es el de la referencia, consistente con su Equals: si
        // fuera el del ID, el HashSet las trataría como una sola.
        HashSet<TestEntity> entities = new HashSet<TestEntity>();

        // Act.
        entities.Add(new TestEntity(0));
        entities.Add(new TestEntity(0));

        // Assert.
        entities.Should().HaveCount(2);
    }

    [Fact]
    public void IsTransient_WhenIntIdIsZero_ShouldBeTrue()
    {
        // Arrange.
        TestEntity entity = new TestEntity(0);

        // Act.
        bool isTransient = entity.IsTransient();

        // Assert.
        isTransient.Should().BeTrue();
    }

    [Fact]
    public void IsTransient_WhenGuidIdIsEmpty_ShouldBeTrue()
    {
        // Arrange.
        TestAggregate aggregate = new TestAggregate(Guid.Empty);

        // Act.
        bool isTransient = aggregate.IsTransient();

        // Assert.
        isTransient.Should().BeTrue();
    }

    [Fact]
    public void IsTransient_WhenIdIsAssigned_ShouldBeFalse()
    {
        // Arrange.
        TestEntity entity = new TestEntity(7);

        // Act.
        bool isTransient = entity.IsTransient();

        // Assert.
        isTransient.Should().BeFalse();
    }

    #endregion
}
