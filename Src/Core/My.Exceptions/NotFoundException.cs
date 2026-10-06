namespace My.Exceptions;

/// <summary>
/// Exception to represent that a requested resource was not found.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1032:Implement standard exception constructors",
    Justification = "We want to use this constructor only.")]
public class NotFoundException : Exception
{
    #region Declarations

    private const string DefaultMessage = "The requested resource was not found.";

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    public NotFoundException()
        : base(DefaultMessage)
    {
        SetDefaultErrorData();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public NotFoundException(string message)
        : base(message ?? DefaultMessage)
    {
        SetDefaultErrorData();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public NotFoundException(string message, Exception innerException)
        : base(message ?? DefaultMessage, innerException)
    {
        SetDefaultErrorData();
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Sets the default error code and error type for not-found errors.
    /// </summary>
    private void SetDefaultErrorData()
    {
        Data[ExDataKey.ErrorCode] = ExErrorCodeCore.ErrNotFound;
        Data[ExDataKey.ErrorType] = ErrorType.NotFound;
    }

    #endregion
}
