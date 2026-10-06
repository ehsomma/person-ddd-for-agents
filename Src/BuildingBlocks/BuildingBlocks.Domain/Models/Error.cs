using My.Exceptions;

namespace BuildingBlocks.Domain.Models;

/// <summary>
/// Represents a concrete domain error.
/// </summary>
public sealed class Error
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="type">The type of the error (helps to resolve the HttpStatusCode in http requests).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="code"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="code"/> is empty or consists only of white-space characters.</exception>
    public Error(string code, string message, ErrorType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        Code = code;
        Message = message;
        Type = type;
    }

    #endregion

    #region Properties

    /// <summary>Gets the error code.</summary>
    public string Code { get; }

    /// <summary>Gets the error message.</summary>
    public string Message { get; }

    /// <summary>Gets the type of the error (helps to resolve the HttpStatusCode in http requests).</summary>
    public ErrorType Type { get; }

    #endregion

    #region Public methods

    /// <summary>
    /// Implicit operator.
    /// </summary>
    /// <param name="error">The <see cref="Error"/>.</param>
    public static implicit operator string(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return error.Code;
    }

    /// <summary>
    /// Returns the error code.
    /// </summary>
    /// <returns>The error code.</returns>
    public override string ToString()
    {
        return Code;
    }

    #endregion
}
