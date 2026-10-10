using FluentValidation;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Api.V1.Person.Validators;

/// <summary>
/// Represents the <see cref="Dto.PersonalAsset"/> validator.
/// </summary>
internal sealed class PersonalAssetValidator : AbstractValidator<Dto.PersonalAsset>
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonalAssetValidator"/> class.
    /// </summary>
    public PersonalAssetValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(255);

        // TODO: Esta regla hoy solo existe acá: Money/PersonalAsset aceptan cualquier valor. Si es una
        // regla de negocio, debería estar en el dominio.
        RuleFor(x => x.Value).GreaterThan(0);
    }

    #endregion
}
