using FluentValidation.TestHelper;
using Records.Persons.Api.V1.Person.Validators;
using Records.Persons.TestData;
using Xunit;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Api.V1.UnitTests.Person.Validators;

/// <summary>
/// Tests del validator <see cref="PersonalAssetValidator"/>: la descripción obligatoria, de 1 a 255
/// caracteres, y el valor mayor a 0.
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
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Validate_WhenValueIsNotPositive_ShouldHaveError(double value)
    {
        // Arrange.
        // NOTE: Esta regla hoy solo existe en el validator (ver el TODO de PersonalAssetValidator):
        // Money acepta cualquier monto.
        Dto.PersonalAsset personalAssetDto = new Dto.PersonalAsset { Value = (decimal)value };

        // Act.
        TestValidationResult<Dto.PersonalAsset> result = _validator.TestValidate(
            personalAssetDto,
            options => options.IncludeProperties(x => x.Value));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void Validate_WhenValueExceedsTheColumnPrecision_ShouldNotHaveError()
    {
        // Arrange.
        // TODO: La columna PersonalAssets.Value es decimal(9, 2): el máximo es 9.999.999,99. Ni el
        // validator ni Money tienen un máximo, así que un valor de 10.000.000 pasa los dos y falla al
        // insertar (500 en lugar de 400). Además, los decimales de más (p.ej. 10,555) SQL Server los
        // redondea sin avisar. Hay que decidir si el validator (o el dominio) limita el valor y la
        // cantidad de decimales; si se agrega, este test pasa a esperar el error.
        Dto.PersonalAsset personalAssetDto = new Dto.PersonalAsset { Value = 10_000_000M };

        // Act.
        TestValidationResult<Dto.PersonalAsset> result = _validator.TestValidate(
            personalAssetDto,
            options => options.IncludeProperties(x => x.Value));

        // Assert.
        result.ShouldNotHaveValidationErrorFor(x => x.Value);
    }

    #endregion
}
