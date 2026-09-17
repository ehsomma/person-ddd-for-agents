#region Usings

using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using My.Exceptions;
using Records.Shared.Contracts;

#endregion

namespace Records.Shared.Infra.Http;

/// <summary>
/// Represents the global exception handler that can be added to the application's request.
/// </summary>
/// <param name="logger">Logger injection.</param>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    #region Public methods

    /// <inheritdoc/>
    [SuppressMessage(
        "Naming",
        "CA1725:Los nombres de parámetro deben coincidir con la declaración base",
        Justification = "Permitido usar ex para excepciones.")]
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception ex,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(ex);

        string errorCode = ex.GetDataValue(ExDataKey.ErrorCode) ?? "ERR";
        string errorGroup = ex.GetDataValue(ExDataKey.ErrorGroup) ?? string.Empty;
        string errorMessage = ex.Message;
        ////HttpStatusCode httpStatusCode = ResolveHttpStatusCode(ex); // Sin DDD.
        HttpStatusCode httpStatusCode = ResolveHttpStatusCode(errorCode, errorGroup); // Con DDD.

        string? errorLogId = null;
        ex.SetTimeStamp(); // Solo la agrega si no tiene previamente.

        // If the httpStatusCode is 500, it should log.
        if (httpStatusCode == HttpStatusCode.InternalServerError)
        {
            errorLogId = Guid.NewGuid().ToString("N");

            // This is what we will see in the log (logId and the full exception) if the
            // exception should be logged.
            ex.Data[ExDataKey.ErrorLogId] = errorLogId;
            logger.LogError(ex, "An exception occurred (id: {ErrorLogId}): {Message}", errorLogId, ex.Message);

            errorMessage = $"An exception occurred. Please, provide this error log id to technical support (logId: {errorLogId}).";
        }

        DateTime timeStampUtc = ex.GetTimeStamp();

        // This is what the user will see.
        ErrorResponse errorResponse = new(
            errorCode,
            errorMessage,
            (int)httpStatusCode,
            timeStampUtc,
            errorLogId,
            MapValidationErrors(GetValidationErrors(ex)));

        // Prepare the response.
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)httpStatusCode;
        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

        return true; // manejada, cortá acá.
    }

    #endregion

    #region Private methods

    /// <summary>
    /// If the exception is ValidationException, gets the ValidationErros, otherwise return null.
    /// </summary>
    /// <param name="ex">The exception.</param>
    /// <returns>The validation erros.</returns>
    private IReadOnlyCollection<ValidationError>? GetValidationErrors(Exception ex)
    {
        return (ex is ValidationException)
            ? ((ValidationException)ex).ValidationErrors ?? null
            : null;
    }

    // Map IReadOnlyCollection<ValidationError> to

    /// <summary>
    /// Maps an IReadOnlyCollection of <see cref="ValidationError"/> to a IReadOnlyCollection
    /// of <see cref="ValidationErrorResponse"/>.
    /// </summary>
    /// <param name="validationErrors">The ValidationErros.</param>
    /// <returns>The validation erros dtos.</returns>
    private IReadOnlyCollection<ValidationErrorResponse>? MapValidationErrors(IReadOnlyCollection<ValidationError>? validationErrors)
    {
        IReadOnlyCollection<ValidationErrorResponse>? validationErrorsResponses = null;

        if (validationErrors != null)
        {
            validationErrorsResponses = validationErrors
                .Select(failure => new ValidationErrorResponse(
                    failure.PropertyName,
                    failure.ErrorMessage,
                    failure.AttemptedValue))
                .ToList().AsReadOnly();
        }

        return validationErrorsResponses;
    }

    /// <summary>
    /// Resolves the <see cref="HttpStatusCode"/> from the exception type.
    /// </summary>
    /// <param name="ex">The exception.</param>
    /// <returns>The resolved <see cref="HttpStatusCode"/>.</returns>
    [SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Queda por si en algún momento resolvermos el status code por tipo de excepción.")]
    private HttpStatusCode ResolveHttpStatusCode(Exception ex)
    {
        HttpStatusCode ret;

        switch (ex)
        {
            case ValidationException:
                ret = HttpStatusCode.BadRequest;
                break;

            case UnauthorizedAccessException:
                ret = HttpStatusCode.Unauthorized;
                break;

            case ForbiddenException:
                ret = HttpStatusCode.Forbidden;
                break;

            case NotFoundException:
                ret = HttpStatusCode.NotFound;
                break;

            default:
                ret = HttpStatusCode.InternalServerError;
                break;
        }

        return ret;
    }

    /// <summary>
    /// Resolves the <see cref="HttpStatusCode"/> from the <paramref name="errorCode"/> or
    /// <paramref name="errorGroup"/> specified.
    /// </summary>
    /// <param name="errorCode">The custom errorCode (got from the exception).</param>
    /// <param name="errorGroup">The custom errorGroup (got from the exception).</param>
    /// <returns>The resolved <see cref="HttpStatusCode"/>.</returns>
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Kept intentionally as DDD reference, see summary.")]
    private HttpStatusCode ResolveHttpStatusCode(string errorCode, string errorGroup)
    {
        HttpStatusCode ret;

        if (string.IsNullOrWhiteSpace(errorCode))
        {
            ret = HttpStatusCode.InternalServerError; // 500.
        }
        else
        {
            if (string.IsNullOrWhiteSpace(errorGroup))
            {
                errorGroup = string.Empty;
            }

            if (errorCode == ExErrorCodeCore.ErrValidation
                || errorCode.Contains(".VAL.", StringComparison.InvariantCultureIgnoreCase)
                || errorCode.Contains(".VALIDATION", StringComparison.CurrentCultureIgnoreCase)
                || errorGroup == "Validation"
                || errorGroup == "DomainValidation")
            {
                ret = HttpStatusCode.BadRequest; // 400.
            }
            else if (errorCode.Contains(".AUTH.", StringComparison.InvariantCultureIgnoreCase)
                     || errorGroup == "Auth")
            {
                ret = HttpStatusCode.Unauthorized; // 401.
            }
            else if (errorCode.Contains(".FORB.", StringComparison.InvariantCultureIgnoreCase)
                || errorCode.Contains(".DOM.", StringComparison.InvariantCultureIgnoreCase)
                || errorCode.Contains(".BNS.", StringComparison.InvariantCultureIgnoreCase)
                || errorGroup == "Domain"
                || errorGroup == "Business")
            {
                ret = HttpStatusCode.Forbidden; // 403.
            }
            else
            {
                ret = HttpStatusCode.InternalServerError; // 500.
            }
        }

        return ret;
    }

    #endregion
}
