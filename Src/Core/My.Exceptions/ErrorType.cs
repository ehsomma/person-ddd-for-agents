namespace My.Exceptions;

/// <summary>
/// Error types (helps to resolve the HttpStatusCode in http requests).
/// </summary>
public enum ErrorType
{
    /// <summary>Indicates that the error is related to validation.</summary>
    Validation = 1,

    /// <summary>Indicates that the error is related to a missing or invalid authentication.</summary>
    Unauthorized = 2,

    /// <summary>Indicates that the error is related to a business rule violation.</summary>
    Forbidden = 3,

    /// <summary>Indicates that the error is related to a not found entity.</summary>
    NotFound = 4,

    /// <summary>Indicates that the error is related to a conflict.</summary>
    Conflict = 5,

    /// <summary>Indicates that the error is related to an internal failure.</summary>
    Failure = 6,
}
