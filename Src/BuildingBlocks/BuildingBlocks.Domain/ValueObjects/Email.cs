namespace BuildingBlocks.Domain.ValueObjects;

/// <summary>
/// Represents the Email value object.
/// </summary>
public sealed class Email : StringValueObject
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Email"/> value object.
    /// </summary>
    /// <param name="value">The email address.</param>
    // Regex pragmática, no la de la RFC 5322: frena los errores de tipeo obvios y deja pasar los emails
    // reales (la validación real de un email es mandarle un mail). Una sola "@", sin espacios y con al
    // menos un punto en el dominio:
    //   john+news@gmail.com   → válido ✓ (también "_" y "'" en la parte local)
    //   john@gmail            → inválido ✗ (sin punto en el dominio)
    //   john@gmail..com       → inválido ✗ (punto vacío en el dominio)
    //   john@doe@gmail.com    → inválido ✗ (dos "@")
    // NOTE: El PersonValidator de la API usa la misma regex escrita a mano (se decidió no compartirla
    // para no acoplar la API al dominio). Si la cambiás acá, cambiala también ahí:
    // PersonValidatorTests.Validate_WhenEmailIsChecked_ShouldAcceptTheSameEmailsAsTheDomain falla si
    // las dos reglas no aceptan los mismos emails.
    private Email(string? value)
        : base(value, "Email", 362, 1, @"^[^@\s]+@[^@\s.]+(\.[^@\s.]+)+$")
    {
    }

    /// <summary>
    /// Build a new <see cref="Email"/> instance based on the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The email address.</param>
    /// <returns>The value object.</returns>
    public static Email Build(string? value)
    {
        return new Email(value);
    }

    #endregion
}
