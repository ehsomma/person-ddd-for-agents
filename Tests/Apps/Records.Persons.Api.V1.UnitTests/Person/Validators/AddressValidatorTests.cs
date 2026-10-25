using FluentValidation.TestHelper;
using Records.Persons.Api.V1.Person.Validators;
using Records.Persons.TestData;
using Xunit;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Api.V1.UnitTests.Person.Validators;

/// <summary>
/// Tests del validator <see cref="AddressValidator"/>: la primera línea es obligatoria y el resto de
/// los datos son opcionales, pero si vienen tienen que respetar el largo.
/// <para>
/// Igual que en <see cref="PersonValidatorTests"/>, cada regla se prueba con un DTO que solo tiene
/// ese dato y con <c>IncludeProperties</c>.
/// </para>
/// </summary>
public sealed class AddressValidatorTests
{
    #region Declarations

    // NOTE: Los largos tienen que coincidir con los de los value objects del dominio (StreetLine,
    // StreetLine2, City, State y CountryName).
    private const int StreetLineMaxLength = 60;

    private const int CityAndStateMaxLength = 50;

    private const int CountryMaxLength = 160;

    private readonly AddressValidator _validator = new AddressValidator();

    #endregion

    #region Public methods

    [Fact]
    public void Validate_WhenAddressIsValid_ShouldNotHaveErrors()
    {
        // Arrange.
        Dto.Address addressDto = PersonDtoMother.ValidPerson(Guid.NewGuid()).Address!;

        // Act.
        TestValidationResult<Dto.Address> result = _validator.TestValidate(addressDto);

        // Assert.
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenOnlyStreetLine1IsSpecified_ShouldNotHaveErrors()
    {
        // Arrange.
        // Todos los demás datos son opcionales: con null no corre la regla del largo.
        Dto.Address addressDto = new Dto.Address { StreetLine1 = "Av. Corrientes 1234" };

        // Act.
        TestValidationResult<Dto.Address> result = _validator.TestValidate(addressDto);

        // Assert.
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_WhenStreetLine1IsNullOrEmpty_ShouldHaveError(string? streetLine1)
    {
        // Arrange.
        Dto.Address addressDto = new Dto.Address { StreetLine1 = streetLine1 };

        // Act.
        TestValidationResult<Dto.Address> result = _validator.TestValidate(
            addressDto,
            options => options.IncludeProperties(x => x.StreetLine1));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.StreetLine1);
    }

    [Fact]
    public void Validate_WhenStreetLine1HasMaxLength_ShouldNotHaveError()
    {
        // Arrange.
        Dto.Address addressDto = new Dto.Address { StreetLine1 = new string('a', StreetLineMaxLength) };

        // Act.
        TestValidationResult<Dto.Address> result = _validator.TestValidate(
            addressDto,
            options => options.IncludeProperties(x => x.StreetLine1));

        // Assert.
        result.ShouldNotHaveValidationErrorFor(x => x.StreetLine1);
    }

    [Fact]
    public void Validate_WhenStreetLine1ExceedsMaxLength_ShouldHaveError()
    {
        // Arrange.
        Dto.Address addressDto = new Dto.Address { StreetLine1 = new string('a', StreetLineMaxLength + 1) };

        // Act.
        TestValidationResult<Dto.Address> result = _validator.TestValidate(
            addressDto,
            options => options.IncludeProperties(x => x.StreetLine1));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.StreetLine1);
    }

    [Fact]
    public void Validate_WhenStreetLine2IsEmpty_ShouldHaveError()
    {
        // Arrange.
        // Opcional, pero si viene no puede estar vacía (Length(1, 60)), igual que el value object
        // StreetLine2: null es "no vino" y "" es un dato inválido.
        Dto.Address addressDto = new Dto.Address { StreetLine2 = string.Empty };

        // Act.
        TestValidationResult<Dto.Address> result = _validator.TestValidate(
            addressDto,
            options => options.IncludeProperties(x => x.StreetLine2));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.StreetLine2);
    }

    [Fact]
    public void Validate_WhenStreetLine2ExceedsMaxLength_ShouldHaveError()
    {
        // Arrange.
        Dto.Address addressDto = new Dto.Address { StreetLine2 = new string('a', StreetLineMaxLength + 1) };

        // Act.
        TestValidationResult<Dto.Address> result = _validator.TestValidate(
            addressDto,
            options => options.IncludeProperties(x => x.StreetLine2));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.StreetLine2);
    }

    [Fact]
    public void Validate_WhenCityIsEmpty_ShouldHaveError()
    {
        // Arrange.
        Dto.Address addressDto = new Dto.Address { City = string.Empty };

        // Act.
        TestValidationResult<Dto.Address> result = _validator.TestValidate(
            addressDto,
            options => options.IncludeProperties(x => x.City));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void Validate_WhenCityHasMaxLength_ShouldNotHaveError()
    {
        // Arrange.
        Dto.Address addressDto = new Dto.Address { City = new string('a', CityAndStateMaxLength) };

        // Act.
        TestValidationResult<Dto.Address> result = _validator.TestValidate(
            addressDto,
            options => options.IncludeProperties(x => x.City));

        // Assert.
        result.ShouldNotHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void Validate_WhenCityExceedsMaxLength_ShouldHaveError()
    {
        // Arrange.
        Dto.Address addressDto = new Dto.Address { City = new string('a', CityAndStateMaxLength + 1) };

        // Act.
        TestValidationResult<Dto.Address> result = _validator.TestValidate(
            addressDto,
            options => options.IncludeProperties(x => x.City));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void Validate_WhenStateExceedsMaxLength_ShouldHaveError()
    {
        // Arrange.
        Dto.Address addressDto = new Dto.Address { State = new string('a', CityAndStateMaxLength + 1) };

        // Act.
        TestValidationResult<Dto.Address> result = _validator.TestValidate(
            addressDto,
            options => options.IncludeProperties(x => x.State));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.State);
    }

    [Fact]
    public void Validate_WhenCountryIsEmpty_ShouldNotHaveError()
    {
        // Arrange.
        // A diferencia de City o State, el país vacío es válido (MaximumLength sin mínimo), igual que
        // el value object CountryName (mínimo 0).
        Dto.Address addressDto = new Dto.Address { Country = string.Empty };

        // Act.
        TestValidationResult<Dto.Address> result = _validator.TestValidate(
            addressDto,
            options => options.IncludeProperties(x => x.Country));

        // Assert.
        result.ShouldNotHaveValidationErrorFor(x => x.Country);
    }

    [Fact]
    public void Validate_WhenCountryExceedsMaxLength_ShouldHaveError()
    {
        // Arrange.
        // NOTE: Coincide con el value object CountryName (160), pero no con la columna
        // Addresses.Country (nvarchar(60)): ver el TODO de CountryNameTests.
        Dto.Address addressDto = new Dto.Address { Country = new string('a', CountryMaxLength + 1) };

        // Act.
        TestValidationResult<Dto.Address> result = _validator.TestValidate(
            addressDto,
            options => options.IncludeProperties(x => x.Country));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Country);
    }

    [Fact]
    public void Validate_WhenLatLngIsInvalid_ShouldHaveErrorFromLatLngValidator()
    {
        // Arrange.
        // Verifica que el LatLngValidator está enganchado (SetValidator): sus reglas se prueban en
        // LatLngValidatorTests.
        Dto.Address addressDto = new Dto.Address
        {
            StreetLine1 = "Av. Corrientes 1234",
            LatLng = new Dto.LatLng { Lat = 91M, Lng = 0M },
        };

        // Act.
        TestValidationResult<Dto.Address> result = _validator.TestValidate(addressDto);

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.LatLng!.Lat);
    }

    #endregion
}
