using System.Diagnostics.CodeAnalysis;

namespace Records.Shared.Contracts;

/// <summary>
/// Represents an error response when an http operation fails either for an internal error
/// or a business exception.
/// </summary>
[SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1629:DocumentationTextMustEndWithAPeriod",
    Justification = "Para permitir omitir el punto final en el tag <example> usado por OpenAPI.")]
public class ErrorResponse
{
    #region Contructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorResponse"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error description.</param>
    /// <param name="httpSatusCode">The http status code.</param>
    /// <param name="timeStampUtc">The timestamp when the error was produced.</param>
    /// <param name="logId">An identifier of the error log so that it can be sent to technical support and with this the log of the complete exception can be searched (for internal errors).</param>
    /// <param name="validationErrors">A list of validation errors description (for validation errors).</param>
    public ErrorResponse(
        string code,
        string? message,
        int httpSatusCode,
        DateTime timeStampUtc,
        string? logId,
        IReadOnlyCollection<ValidationErrorResponse>? validationErrors = null)
    {
        TimeStampUtc = DateTime.UtcNow;
        Code = code;
        Message = message;
        HttpStatusCode = httpSatusCode;
        TimeStampUtc = timeStampUtc;
        LogId = logId;
        ValidationErrors = validationErrors;
    }

    #endregion

    #region Properties

    /// <summary>The timestamp when the error was produced (UTC).</summary>
    public DateTime TimeStampUtc { get; }

    /// <summary>The error business code.</summary>
    /// <example>ERR.VALIDATION</example>
    public string Code { get; }

    /// <summary>The http status code.</summary>
    /// <example>400</example>
    public int HttpStatusCode { get; }

    /// <summary>The error description.</summary>
    /// <example>Validation failed. See 'ValidationErrors' for more details.</example>
    public string? Message { get; }

    /// <summary>
    /// An identifier of the error log so that it can be sent to technical support and with this the
    /// log of the complete exception can be searched (for internal errors).
    /// </summary>
    /// <example>5B5969D3-3A90-476B-ACF1-563FA849CB7D</example>
    public string? LogId { get; }

    /// <summary>A list of validation errors description (for validation errors).</summary>
    public IReadOnlyCollection<ValidationErrorResponse>? ValidationErrors { get; }

    #endregion
}
