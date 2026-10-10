using FluentValidation;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Api.V1.Person.Validators;

/// <summary>
/// Represents the <see cref="Dto.Address"/> validator.
/// </summary>
internal sealed class AddressValidator : AbstractValidator<Dto.Address>
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="AddressValidator"/> class.
    /// </summary>
    public AddressValidator()
    {
        // NOTE: Los validadores de longitud de FluentValidation dan por válido el null, así que en los
        // opcionales `Length(1, n)` solo aplica cuando viene un valor (igual que StringValueObjectNullable).
        RuleFor(x => x.StreetLine1).NotEmpty().MaximumLength(60);
        RuleFor(x => x.StreetLine2).Length(1, 60);
        RuleFor(x => x.City).Length(1, 50);
        RuleFor(x => x.State).Length(1, 50);
        RuleFor(x => x.Country).MaximumLength(160);
        RuleFor(x => x.LatLng).SetValidator(new LatLngValidator()!);
    }

    #endregion
}
