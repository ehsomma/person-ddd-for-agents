using FluentValidation;
using Records.Persons.Domain.PersonAggregate.Enumerators;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Api.V1.Person.Validators;

/// <summary>
/// Represents the <see cref="Dto.Person"/> validator (used by the `CreatePerson` and `UpdatePerson` requests).
/// </summary>
/// <remarks>
/// Valida el formato del request para devolverle al cliente todos los errores juntos y por campo. Las
/// longitudes replican las de los value objects del dominio, que siguen siendo la última línea de defensa.
/// </remarks>
internal sealed class PersonValidator : AbstractValidator<Dto.Person>
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonValidator"/> class.
    /// </summary>
    public PersonValidator()
    {
        string genderNames = string.Join(", ", Gender.List.Select(gender => gender.Name));

        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(62);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(362).EmailAddress();
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
    /// Checks if the <paramref name="genderName"/> matches one of the <see cref="Gender"/> names (same
    /// comparison as <c>Gender.FromName</c>).
    /// </summary>
    /// <param name="genderName">The gender name to check.</param>
    /// <returns>True if it is a valid gender name, otherwise false.</returns>
    private static bool BeAValidGender(string? genderName)
    {
        bool isValid = Gender.List.Any(gender => string.Equals(gender.Name, genderName, StringComparison.OrdinalIgnoreCase));

        return isValid;
    }

    #endregion
}
