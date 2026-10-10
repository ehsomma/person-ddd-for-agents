using FluentValidation;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Api.V1.Person.Validators;

/// <summary>
/// Represents the <see cref="Dto.LatLng"/> validator.
/// </summary>
internal sealed class LatLngValidator : AbstractValidator<Dto.LatLng>
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="LatLngValidator"/> class.
    /// </summary>
    public LatLngValidator()
    {
        RuleFor(x => x.Lat).NotNull().InclusiveBetween(-90, 90);
        RuleFor(x => x.Lng).NotNull().InclusiveBetween(-180, 180);
    }

    #endregion
}
