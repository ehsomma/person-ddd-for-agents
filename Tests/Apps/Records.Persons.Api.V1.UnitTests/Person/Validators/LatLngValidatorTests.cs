using FluentValidation.TestHelper;
using Records.Persons.Api.V1.Person.Validators;
using Xunit;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Api.V1.UnitTests.Person.Validators;

/// <summary>
/// Tests del validator <see cref="LatLngValidator"/>: las dos coordenadas son obligatorias, la latitud
/// de -90 a 90 y la longitud de -180 a 180 (límites incluidos), igual que el value object LatLng.
/// </summary>
public sealed class LatLngValidatorTests
{
    #region Declarations

    private readonly LatLngValidator _validator = new LatLngValidator();

    #endregion

    #region Public methods

    [Theory]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    [InlineData(-34.603722, -58.381592)]
    public void Validate_WhenCoordinatesAreInRange_ShouldNotHaveErrors(double lat, double lng)
    {
        // Arrange.
        // InlineData no acepta decimal (no es una constante de atributo): se convierte acá.
        Dto.LatLng latLngDto = new Dto.LatLng { Lat = (decimal)lat, Lng = (decimal)lng };

        // Act.
        TestValidationResult<Dto.LatLng> result = _validator.TestValidate(latLngDto);

        // Assert.
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(-90.000001)]
    [InlineData(90.000001)]
    public void Validate_WhenLatIsOutOfRange_ShouldHaveError(double lat)
    {
        // Arrange.
        Dto.LatLng latLngDto = new Dto.LatLng { Lat = (decimal)lat, Lng = 0M };

        // Act.
        TestValidationResult<Dto.LatLng> result = _validator.TestValidate(latLngDto);

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Lat);
    }

    [Theory]
    [InlineData(-180.000001)]
    [InlineData(180.000001)]
    public void Validate_WhenLngIsOutOfRange_ShouldHaveError(double lng)
    {
        // Arrange.
        Dto.LatLng latLngDto = new Dto.LatLng { Lat = 0M, Lng = (decimal)lng };

        // Act.
        TestValidationResult<Dto.LatLng> result = _validator.TestValidate(latLngDto);

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Lng);
    }

    [Fact]
    public void Validate_WhenOnlyOneCoordinateIsSpecified_ShouldHaveError()
    {
        // Arrange.
        // Si viene el objeto LatLng, tienen que venir las dos: una dirección sin coordenadas manda
        // LatLng en null, no un LatLng a medias.
        Dto.LatLng latLngDto = new Dto.LatLng { Lat = -34.603722M, Lng = null };

        // Act.
        TestValidationResult<Dto.LatLng> result = _validator.TestValidate(latLngDto);

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Lng);
    }

    #endregion
}
