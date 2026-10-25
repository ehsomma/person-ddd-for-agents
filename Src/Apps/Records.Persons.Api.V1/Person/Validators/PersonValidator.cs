using FluentValidation;
using Records.Persons.Domain.PersonAggregate.Enumerators;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Api.V1.Person.Validators;

/// <summary>
/// Representa el validator de <see cref="Dto.Person"/> (lo usan los requests de `CreatePerson` y
/// `UpdatePerson`).
/// </summary>
/// <remarks>
/// Valida el formato del request para devolverle al cliente todos los errores juntos y por campo.
/// Las longitudes replican las de los value objects del dominio, que siguen siendo la última línea
/// de defensa.
/// </remarks>
internal sealed class PersonValidator : AbstractValidator<Dto.Person>
{
    #region Constructor

    /// <summary>Inicializa una nueva instancia de <see cref="PersonValidator"/>.</summary>
    public PersonValidator()
    {
        string genderNames = string.Join(", ", Gender.List.Select(gender => gender.Name));

        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(62);

        // NOTE: Misma regex que el value object Email del dominio (ver ahí qué acepta y qué no), escrita
        // a mano para no acoplar la API al dominio. Si la cambiás acá, cambiala también ahí:
        // PersonValidatorTests.Validate_WhenEmailIsChecked_ShouldAcceptTheSameEmailsAsTheDomain falla
        // si las dos reglas no aceptan los mismos emails. No se usa EmailAddress() de FluentValidation
        // porque solo pide una "@" y dejaba pasar emails que el dominio rechaza.
        RuleFor(x => x.Email).NotEmpty().MaximumLength(362).Matches(@"^[^@\s]+@[^@\s.]+(\.[^@\s.]+)+$");

        RuleFor(x => x.Phone).MaximumLength(60);
        RuleFor(x => x.Gender)
            .NotEmpty()
            .Must(BeAValidGender)
            .WithMessage($"'{{PropertyName}}' must be one of: {genderNames}.");

        RuleFor(x => x.Address).NotNull().SetValidator(new AddressValidator()!);
        RuleForEach(x => x.PersonalAssets).SetValidator(new PersonalAssetValidator());
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Verifica si <paramref name="genderName"/> coincide con alguno de los nombres de
    /// <see cref="Gender"/>, con la misma comparación que <c>Gender.FromName</c> (sin distinguir
    /// mayúsculas de minúsculas).
    /// </summary>
    /// <param name="genderName">El nombre del género a verificar.</param>
    /// <returns><see langword="true"/> si es un nombre de género válido; si no, <see langword="false"/>.</returns>
    private static bool BeAValidGender(string? genderName)
    {
        bool isValid = Gender.List.Any(gender => string.Equals(gender.Name, genderName, StringComparison.OrdinalIgnoreCase));

        return isValid;
    }

    #endregion
}
