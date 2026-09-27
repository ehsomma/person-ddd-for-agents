namespace Records.Shared.Domain.Models;

/// <summary>
/// Defines an auditable entity.
/// </summary>
public interface IEntityAuditable
{
    #region Properties

    /// <summary>
    /// Gets the date and time (UTC) the entity was created on.
    /// </summary>
    DateTime? CreatedOnUtc { get; }

    /// <summary>
    /// Gets the date and time (UTC) the entity was modified on.
    /// </summary>
    DateTime? UpdatedOnUtc { get; }

    #endregion
}
