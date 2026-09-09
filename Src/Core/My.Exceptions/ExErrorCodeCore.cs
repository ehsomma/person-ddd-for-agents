namespace My.Exceptions;

/// <summary>
/// Error codes.
/// </summary>
public static class ExErrorCodeCore
{
    #region Declarations

    /// <summary>Unhandled exception.</summary>
    public const string Err = "ERR";

    /// <summary>Entity or argument validation.</summary>
    public const string ErrValidation = "ERR.VALIDATION";

    /// <summary>The operation is forbidden (business rule or access restriction).</summary>
    public const string ErrForbidden = "ERR.FORBIDDEN";

    /// <summary>The requested resource was not found.</summary>
    public const string ErrNotFound = "ERR.NOTFOUND";

    #endregion
}
