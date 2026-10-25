using FluentValidation.TestHelper;
using Records.Persons.Api.V1.Person.Validators;
using Records.Persons.TestData;
using Xunit;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Api.V1.UnitTests.Person.Validators;

/// <summary>
/// Tests del validator <see cref="PersonalAssetValidator"/>: la descripción obligatoria, de 1 a 255
/// caracteres, y el valor de 0 a 999.999.999,99 con hasta 2 decimales.
/// </summary>
public sealed class PersonalAssetValidatorTests
{
    #region Declarations

    // NOTE: Tiene que coincidir con el value object PersonalAssetDescription.
    private const int DescriptionMaxLength = 255;

    private readonly PersonalAssetValidator _validator = new PersonalAssetValidator();

    #endregion

    #region Public methods

    [Fact]
    public void Validate_WhenPersonalAssetIsValid_ShouldNotHaveErrors()
    {
        // Arrange.
        Dto.PersonalAsset personalAssetDto = PersonDtoMother.PersonalAsset();

        // Act.
        TestValidationResult<Dto.PersonalAsset> result = _validator.TestValidate(personalAssetDto);

        // Assert.
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_WhenDescriptionIsNullOrEmpty_ShouldHaveError(string? description)
    {
        // Arrange.
        Dto.PersonalAsset personalAssetDto = new Dto.PersonalAsset { Description = description };

        // Act.
        TestValidationResult<Dto.PersonalAsset> result = _validator.TestValidate(
            personalAssetDto,
            options => options.IncludeProperties(x => x.Description));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WhenDescriptionHasMaxLength_ShouldNotHaveError()
    {
        // Arrange.
        Dto.PersonalAsset personalAssetDto = new Dto.PersonalAsset { Description = new string('a', DescriptionMaxLength) };

        // Act.
        TestValidationResult<Dto.PersonalAsset> result = _validator.TestValidate(
            personalAssetDto,
            options => options.IncludeProperties(x => x.Description));

        // Assert.
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WhenDescriptionExceedsMaxLength_ShouldHaveError()
    {
        // Arrange.
        Dto.PersonalAsset personalAssetDto = new Dto.PersonalAsset { Description = new string('a', DescriptionMaxLength + 1) };

        // Act.
        TestValidationResult<Dto.PersonalAsset> result = _validator.TestValidate(
            personalAssetDto,
            options => options.IncludeProperties(x => x.Description));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData(0)] // Un bien sin valor declarado es válido.
    [InlineData(0.01)]
    [InlineData(10.5)]
    [InlineData(999_999_999.99)] // El máximo de la columna decimal(11, 2).
    public void Validate_WhenValueIsInRange_ShouldNotHaveError(double value)
    {
        // Arrange.
        // NOTE: Misma regla que el value object PersonalAssetValue del dominio (ver
        // PersonalAssetValueTests). InlineData no acepta decimal: se convierte acá.
        Dto.PersonalAsset personalAssetDto = new Dto.PersonalAsset { Value = (decimal)value };

        // Act.
        TestValidationResult<Dto.PersonalAsset> result = _validator.TestValidate(
            personalAssetDto,
            options => options.IncludeProperties(x => x.Value));

        // Assert.
        result.ShouldNotHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void Validate_WhenValueIsNegative_ShouldHaveError()
    {
        // Arrange.
        Dto.PersonalAsset personalAssetDto = new Dto.PersonalAsset { Value = -0.01M };

        // Act.
        TestValidationResult<Dto.PersonalAsset> result = _validator.TestValidate(
            personalAssetDto,
            options => options.IncludeProperties(x => x.Value));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void Validate_WhenValueExceedsMaxValue_ShouldHaveError()
    {
        // Arrange.
        // 1.000.000.000 tiene 10 dígitos enteros: no entra en decimal(11, 2), que admite 9.
        Dto.PersonalAsset personalAssetDto = new Dto.PersonalAsset { Value = 1_000_000_000M };

        // Act.
        TestValidationResult<Dto.PersonalAsset> result = _validator.TestValidate(
            personalAssetDto,
            options => options.IncludeProperties(x => x.Value));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Theory]
    [InlineData("10.555")]
    [InlineData("0.001")]
    public void Validate_WhenValueHasMoreThanTwoDecimals_ShouldHaveError(string value)
    {
        // Arrange.
        // Se rechaza en lugar de redondear: si se redondeara (o lo redondeara SQL Server), el valor
        // guardado no sería el que se mandó y nadie se enteraría. El valor llega como string porque
        // InlineData no acepta decimal.
        Dto.PersonalAsset personalAssetDto = new Dto.PersonalAsset
        {
            Value = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture),
        };

        // Act.
        TestValidationResult<Dto.PersonalAsset> result = _validator.TestValidate(
            personalAssetDto,
            options => options.IncludeProperties(x => x.Value));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    #endregion
}
