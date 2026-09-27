using Records.Shared.Domain.Exceptions;
using Records.Shared.Domain.Models;
using Throw;

namespace Records.Shared.Domain.ValueObjects;

/// <summary>
/// A value object base class for strings value objects (nullable).
/// </summary>
/// <remarks>
/// Static methods can not be abstract in C#, so each derived class must implement its own <c>Build</c> method:
/// <code>
/// public static MyValueObject Build(string? value)
/// {
///     MyValueObject valueObject = new(value);
///
///     return valueObject;
/// }
/// </code>
/// </remarks>
public abstract class StringValueObjectNullable : ValueObject
{
    #region Declarations

    /// <summary>Regex always valid (default).</summary>
    private const string RegexValid = ".*";

    private readonly int _maxLength;

    private readonly int _minLength;

    private readonly string _valueName;

    private readonly string _regex;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="StringValueObjectNullable"/> value object.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <param name="valueName">The name to show in the exception if its value is invalid.</param>
    /// <param name="maxLength">The maximum length to validate (send 0 to skip the validation).</param>
    /// <param name="minLength">The minimum length to validate (default 0).</param>
    /// <param name="regex">Regular expression to check (default ".*", anything).</param>
    protected StringValueObjectNullable(
        string? value,
        string valueName,
        int maxLength,
        int minLength = 0,
        string regex = RegexValid)
    {
        _valueName = valueName;
        _maxLength = maxLength;
        _minLength = minLength;
        _regex = regex;

        EnsureIsValid(value);

        // It accept nulls.
        Value = value;
    }

    #endregion

    #region Properties

    /// <summary>Gets the value.</summary>
    public string? Value { get; }

    /// <summary>Gets a value indicating whether the ValueObject has a value other than null or empty.</summary>
    public bool HasValue => !string.IsNullOrEmpty(Value);

    /// <summary>Gets a value indicating whether the ValueObject has not a value.</summary>
    public bool IsEmpty => string.IsNullOrEmpty(Value);

    #endregion

    #region Public methods

    /// <summary>
    /// Implicit operator that returns its value.
    /// </summary>
    /// <param name="valueObject">The value object.</param>
    public static implicit operator string?(StringValueObjectNullable? valueObject)
    {
        return valueObject?.Value;
    }

    /// <inheritdoc />
    public override string? ToString()
    {
        return Value;
    }

    #endregion

    #region Protected methods

    /// <summary>
    /// Asserts that the arguments to create the <see cref="StringValueObjectNullable"/> value object are valid.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <exception cref="DomainValidationException">If some argument is invalid.</exception>
    protected void EnsureIsValid(string? value)
    {
        // INFO: To build regex, ask ChatGpt ;).
        // INFO: To test regex, go: https://regex101.com/.

        try
        {
            // If value is null, it does not need to check others validations.
            // If value is not null, it will check the others validations.
            value?.Throw(paramName: _valueName)
                .IfShorterThan(_minLength)
                .IfLongerThan(_maxLength)
                .IfNotMatches(_regex);
        }
        catch (Exception ex)
        {
            throw new DomainValidationException(ex.Message, ex);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<object> GetEqualityComponents()
    {
        if (Value != null)
        {
            yield return Value;
        }
    }

    #endregion
}
