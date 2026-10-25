using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.ValueObjects;
using Xunit;

namespace BuildingBlocks.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests of the <see cref="LatLng"/> value object: both coordinates required, the latitude from -90
/// to 90 and the longitude from -180 to 180 (limits included).
/// </summary>
public sealed class LatLngTests
{
    #region Public methods

    [Fact]
    public void Build_WhenCoordinatesAreValid_ShouldKeepThem()
    {
        // Arrange.

        // Act.
        LatLng latLng = LatLng.Build(-34.603722M, -58.381592M);

        // Assert.
        latLng.Lat.Should().Be(-34.603722M);
        latLng.Lng.Should().Be(-58.381592M);
    }

    [Theory]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    [InlineData(0, 0)]
    public void Build_WhenCoordinatesAreOnTheLimits_ShouldKeepThem(decimal lat, decimal lng)
    {
        // Arrange.

        // Act.
        LatLng latLng = LatLng.Build(lat, lng);

        // Assert.
        latLng.Lat.Should().Be(lat);
        latLng.Lng.Should().Be(lng);
    }

    [Theory]
    [InlineData(-90.000001, 0)] // Latitud fuera de rango.
    [InlineData(90.000001, 0)]
    [InlineData(0, -180.000001)] // Longitud fuera de rango.
    [InlineData(0, 180.000001)]
    public void Build_WhenCoordinatesAreOutOfRange_ShouldThrowDomainValidationException(double lat, double lng)
    {
        // Arrange.
        // InlineData no acepta decimal (no es una constante de atributo), así que los valores llegan
        // como double y se convierten acá.
        decimal latitude = (decimal)lat;
        decimal longitude = (decimal)lng;

        // Act.
        Action act = () => LatLng.Build(latitude, longitude);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Theory]
    [InlineData(null, -58.381592)]
    [InlineData(-34.603722, null)]
    [InlineData(null, null)]
    public void Build_WhenSomeCoordinateIsNull_ShouldThrowDomainValidationException(double? lat, double? lng)
    {
        // Arrange.
        // Las dos coordenadas son obligatorias: una dirección sin coordenadas no tiene LatLng (queda
        // en null), no un LatLng con una sola.
        decimal? latitude = (decimal?)lat;
        decimal? longitude = (decimal?)lng;

        // Act.
        Action act = () => LatLng.Build(latitude, longitude);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void ToString_WhenCalled_ShouldUseInvariantCulture()
    {
        // Arrange.
        LatLng latLng = LatLng.Build(-34.603722M, -58.381592M);

        // Act.
        // Con la cultura es-AR el separador decimal es la coma: si ToString no usara la cultura
        // invariante, quedaría "-34,603722, -58,381592" y no se podría separar.
        string value = latLng.ToString();

        // Assert.
        value.Should().Be("-34.603722, -58.381592");
    }

    [Fact]
    public void Equals_WhenCoordinatesAreEqual_ShouldBeEqual()
    {
        // Arrange.
        LatLng latLng = LatLng.Build(-34.603722M, -58.381592M);
        LatLng otherLatLng = LatLng.Build(-34.603722M, -58.381592M);

        // Act.
        bool areEqual = latLng == otherLatLng;

        // Assert.
        areEqual.Should().BeTrue();
    }

    #endregion
}
