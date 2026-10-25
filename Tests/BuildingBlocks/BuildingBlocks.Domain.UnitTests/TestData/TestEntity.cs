using BuildingBlocks.Domain.Models;

namespace BuildingBlocks.Domain.UnitTests.TestData;

/// <summary>
/// A minimal <see cref="Entity{TId}"/> with an <see cref="int"/> ID, like the child entities of the
/// modules (where 0 means new).
/// </summary>
internal sealed class TestEntity : Entity<int>
{
    #region Constructor

    /// <summary>Initializes a new instance of the <see cref="TestEntity"/> class.</summary>
    /// <param name="id">The entity ID.</param>
    public TestEntity(int id)
        : base(id)
    {
    }

    #endregion
}
