using BuildingBlocks.Domain.Models;
using My.Exceptions;

namespace BuildingBlocks.Domain.Exceptions;

/// <summary>
/// Represents an exception that occurs in the domain and contains information about the domain <see cref="BuildingBlocks.Domain.Models.Error"/>.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1032:Implement standard exception constructors",
    Justification = "We want to use this constructor only.")]
public sealed class DomainException : Exception
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class.
    /// </summary>
    /// <param name="error">The error containing the information about what happened.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is <see langword="null"/>.</exception>
    public DomainException(Error error)
        : base((error ?? throw new ArgumentNullException(nameof(error))).Message)
    {
        Error = error;

        Data[ExDataKey.ErrorCode] = error.Code;
        Data[ExDataKey.ErrorGroup] = error.Group;
    }

    #endregion

    #region Properties

    /// <summary>Gets the error.</summary>
    public Error Error { get; }

    #endregion
}
