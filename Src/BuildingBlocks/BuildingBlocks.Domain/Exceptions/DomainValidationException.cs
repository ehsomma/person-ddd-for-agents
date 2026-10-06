using My.Exceptions;

namespace BuildingBlocks.Domain.Exceptions;

/// <summary>
/// Base exception to represent validations domain errors.
/// </summary>
/// <remarks>
/// The <see cref="ExDataKey.ErrorCode"/> and <see cref="ExDataKey.ErrorType"/> are set in the base class.
/// </remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1032:Implement standard exception constructors",
    Justification = "We want to use this constructor only.")]
public class DomainValidationException : ValidationException
{
    #region Contructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public DomainValidationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DomainValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    #endregion
}
