using System.Diagnostics.CodeAnalysis;

namespace Records.Shared.Contracts;

/// <summary>
/// Represents a validation error.
/// </summary>
[SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1629:DocumentationTextMustEndWithAPeriod",
    Justification = "Para permitir omitir el punto final en el tag <example> usado por OpenAPI.")]
public class ValidationErrorResponse
{
    #region Contructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationErrorResponse"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the property that have not pass the validation.</param>
    /// <param name="errorMessage">The validation description.</param>
    /// <param name="attemptedValue">The attempted value.</param>
    public ValidationErrorResponse(string propertyName, string errorMessage, object attemptedValue)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        AttemptedValue = attemptedValue;
    }

    #endregion

    #region Properties

    /// <summary>The name of the property that have not pass the validation.</summary>
    /// <example>Country.IataCode</example>
    public string PropertyName { get; }

    /// <summary>The validation description.</summary>
    /// <example>'IataCode' should not be empty.</example>
    public string ErrorMessage { get; }

    /// <summary>The attempted value.</summary>
    public object AttemptedValue { get; }

    #endregion
}
