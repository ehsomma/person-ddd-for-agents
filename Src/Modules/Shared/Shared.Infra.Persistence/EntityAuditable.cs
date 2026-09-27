using Shared.Infra.Persistence.Abstractions;

namespace Shared.Infra.Persistence;

/// <summary>
/// Represents an auditable entity.
/// </summary>
public abstract class EntityAuditable : IEntityAuditable
{
    #region Public methods

    /// <inheritdoc />
    public DateTime? CreatedOnUtc { get; init; }

    /// <inheritdoc />
    public DateTime? UpdatedOnUtc { get; init; }

    #endregion
}
