using BuildingBlocks.Domain.Models;

namespace BuildingBlocks.Domain.UnitTests.TestData;

/// <summary>
/// Another <see cref="Entity{TId}"/> with an <see cref="int"/> ID, to compare entities of different
/// types with the same ID (like an address and a personal asset).
/// </summary>
internal sealed class OtherTestEntity : Entity<int>
{
    #region Constructor

    /// <summary>Initializes a new instance of the <see cref="OtherTestEntity"/> class.</summary>
    /// <param name="id">The entity ID.</param>
    public OtherTestEntity(int id)
        : base(id)
    {
    }

    #endregion
}
