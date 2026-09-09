namespace My.Exceptions;

/// <summary>
/// Exception to represent a controlled business error (access restriction or business
/// rule violation).
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1032:Implement standard exception constructors",
    Justification = "We want to use this constructor only.")]
public class ForbiddenException : Exception
{
    #region Declarations

    private const string DefaultMessage = "The operation is forbidden.";

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    public ForbiddenException()
        : base(DefaultMessage)
    {
        SetDefaultErrorCode();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public ForbiddenException(string message)
        : base(message ?? DefaultMessage)
    {
        SetDefaultErrorCode();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ForbiddenException(string message, Exception innerException)
        : base(message ?? DefaultMessage, innerException)
    {
        SetDefaultErrorCode();
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Sets the default error code for forbidden errors.
    /// </summary>
    private void SetDefaultErrorCode()
    {
        Data[ExDataKey.ErrorCode] = ExErrorCodeCore.ErrForbidden;
    }

    #endregion
}
