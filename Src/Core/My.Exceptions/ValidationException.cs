namespace My.Exceptions;

/// <summary>
/// Base exception to represent entities/models validation errors.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1032:Implement standard exception constructors",
    Justification = "We want to use this constructor only.")]
public class ValidationException : Exception
{
    #region Declarations

    private const string DefaultMessage = "Validation failed. See 'ValidationErrors' for more details.";

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public ValidationException(string message)
        : base(message ?? DefaultMessage)
    {
        SetDefaultErrorData();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="validationErros">Validation errors.</param>
    public ValidationException(IReadOnlyCollection<ValidationError> validationErros)
        : base(DefaultMessage)
    {
        SetDefaultErrorData();
        ValidationErrors = validationErros;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ValidationException(string message, Exception innerException)
        : base(message ?? DefaultMessage, innerException)
    {
        SetDefaultErrorData();
    }

    #endregion

    #region Properties

    /// <summary>Validation error list.</summary>
    public IReadOnlyCollection<ValidationError>? ValidationErrors { get; }

    #endregion

    #region Private methods

    /// <summary>
    /// Sets the default error code and error type for validations.
    /// </summary>
    private void SetDefaultErrorData()
    {
        Data[ExDataKey.ErrorCode] = ExErrorCodeCore.ErrValidation;
        Data[ExDataKey.ErrorType] = ErrorType.Validation;
    }

    #endregion
}
