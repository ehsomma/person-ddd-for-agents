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

        // NOTE: Misma regla que el value object PersonalAssetValue del dominio, escrita a mano para no
        // acoplar la API al dominio: de 0 a 999.999.999,99 y hasta 2 decimales. PrecisionScale(11, 2)
        // es exactamente la columna decimal(11, 2): hasta 9 dígitos enteros y 2 decimales (con
        // ignoreTrailingZeros, 10.50 cuenta como 10.5). Si cambiás la regla, cambiala también en el
        // dominio y en la columna.
        RuleFor(x => x.Value).GreaterThanOrEqualTo(0).PrecisionScale(11, 2, true);
    }

    #endregion
}
