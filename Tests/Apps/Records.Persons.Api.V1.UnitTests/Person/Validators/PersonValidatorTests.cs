using FluentValidation.TestHelper;
using Records.Persons.Api.V1.Person.Validators;
using Records.Persons.TestData;
using Xunit;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Api.V1.UnitTests.Person.Validators;

/// <summary>
/// Tests del validator <see cref="PersonValidator"/>: las reglas del body de los endpoints de alta y
/// modificación de personas.
/// <para>
/// Cada regla se prueba con un DTO que solo tiene el dato que interesa y con
/// <c>IncludeProperties</c>, así corre solo la regla de esa propiedad y el error que aparece es el de
/// ese dato (los otros datos vacíos no suman errores). El DTO completo y válido sale de
/// <see cref="PersonDtoMother"/>.
/// </para>
/// </summary>
public sealed class PersonValidatorTests
{
    #region Declarations

    // NOTE: Los largos tienen que coincidir con los de los value objects del dominio (FullName, Email
    // y PhoneNumber): si el validator deja pasar un dato que el dominio rechaza, el error llega igual
    // como 400, pero con el mensaje técnico del dominio en lugar del de FluentValidation.
    private const int FullNameMaxLength = 62;

    private const int EmailMaxLength = 362;

    private const int PhoneMaxLength = 60;

    private readonly PersonValidator _validator = new PersonValidator();

    #endregion

    #region Public methods

    [Fact]
    public void Validate_WhenPersonIsValid_ShouldNotHaveErrors()
    {
        // Arrange.
        Dto.Person personDto = PersonDtoMother.ValidPerson(
            Guid.NewGuid(),
            new List<Dto.PersonalAsset> { PersonDtoMother.PersonalAsset() });

        // Act.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(personDto);

        // Assert.
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenIdIsEmpty_ShouldHaveError()
    {
        // Arrange.
        Dto.Person personDto = new Dto.Person { Id = Guid.Empty };

        // Act.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(
            personDto,
            options => options.IncludeProperties(x => x.Id));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_WhenFullNameIsNullOrEmpty_ShouldHaveError(string? fullName)
    {
        // Arrange.
        Dto.Person personDto = new Dto.Person { FullName = fullName };

        // Act.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(
            personDto,
            options => options.IncludeProperties(x => x.FullName));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_WhenFullNameHasMaxLength_ShouldNotHaveError()
    {
        // Arrange.
        Dto.Person personDto = new Dto.Person { FullName = new string('a', FullNameMaxLength) };

        // Act.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(
            personDto,
            options => options.IncludeProperties(x => x.FullName));

        // Assert.
        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_WhenFullNameExceedsMaxLength_ShouldHaveError()
    {
        // Arrange.
        Dto.Person personDto = new Dto.Person { FullName = new string('a', FullNameMaxLength + 1) };

        // Act.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(
            personDto,
            options => options.IncludeProperties(x => x.FullName));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("johndoe")]
    [InlineData("johndoe@")]
    public void Validate_WhenEmailIsMissingOrInvalid_ShouldHaveError(string? email)
    {
        // Arrange.
        Dto.Person personDto = new Dto.Person { Email = email };

        // Act.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(
            personDto,
            options => options.IncludeProperties(x => x.Email));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WhenEmailHasMaxLength_ShouldNotHaveError()
    {
        // Arrange.
        // El email entero tiene el largo pedido (ver BuildEmail).
        Dto.Person personDto = new Dto.Person { Email = BuildEmail(EmailMaxLength) };

        // Act.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(
            personDto,
            options => options.IncludeProperties(x => x.Email));

        // Assert.
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WhenEmailExceedsMaxLength_ShouldHaveError()
    {
        // Arrange.
        Dto.Person personDto = new Dto.Person { Email = BuildEmail(EmailMaxLength + 1) };

        // Act.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(
            personDto,
            options => options.IncludeProperties(x => x.Email));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("john+news@gmail.com")]
    [InlineData("john_doe@gmail.com")]
    public void Validate_WhenEmailIsRejectedByTheDomainRegex_ShouldNotHaveError(string email)
    {
        // Arrange.
        // TODO: EmailAddress() de FluentValidation solo pide una "@" con algo antes y después, y la
        // regex del value object Email rechaza "+" y "_" en la parte local. Estos emails pasan el
        // validator y los rechaza el dominio (400 con el mensaje técnico de la regex). Se resuelve
        // junto con el TODO de la regex en EmailTests: si se amplía la regex, este test queda bien
        // tal como está; si no, el validator tiene que usar la misma regex.
        Dto.Person personDto = new Dto.Person { Email = email };

        // Act.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(
            personDto,
            options => options.IncludeProperties(x => x.Email));

        // Assert.
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WhenPhoneHasMaxLength_ShouldNotHaveError()
    {
        // Arrange.
        // NOTE: Coincide con el value object PhoneNumber (60), pero no con la columna Persons.Phone
        // (nvarchar(20)): ver el TODO de PhoneNumberTests.
        Dto.Person personDto = new Dto.Person { Phone = new string('1', PhoneMaxLength) };

        // Act.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(
            personDto,
            options => options.IncludeProperties(x => x.Phone));

        // Assert.
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Validate_WhenPhoneExceedsMaxLength_ShouldHaveError()
    {
        // Arrange.
        Dto.Person personDto = new Dto.Person { Phone = new string('1', PhoneMaxLength + 1) };

        // Act.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(
            personDto,
            options => options.IncludeProperties(x => x.Phone));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Validate_WhenPhoneIsNull_ShouldNotHaveError()
    {
        // Arrange.

        // Act.
        // El teléfono es opcional.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(
            new Dto.Person { Phone = null },
            options => options.IncludeProperties(x => x.Phone));

        // Assert.
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Theory]
    [InlineData("Female")]
    [InlineData("female")]
    [InlineData("OTHER")]
    public void Validate_WhenGenderIsValid_ShouldNotHaveError(string gender)
    {
        // Arrange.
        // Sin distinguir mayúsculas, igual que Gender.FromName en el dominio.
        Dto.Person personDto = new Dto.Person { Gender = gender };

        // Act.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(
            personDto,
            options => options.IncludeProperties(x => x.Gender));

        // Assert.
        result.ShouldNotHaveValidationErrorFor(x => x.Gender);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Unknown")]
    public void Validate_WhenGenderIsMissingOrInvalid_ShouldHaveError(string? gender)
    {
        // Arrange.
        Dto.Person personDto = new Dto.Person { Gender = gender };

        // Act.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(
            personDto,
            options => options.IncludeProperties(x => x.Gender));

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Gender);
    }

    [Fact]
    public void Validate_WhenAddressIsNull_ShouldHaveError()
    {
        // Arrange.
        Dto.Person personDto = PersonDtoMother.ValidPerson(Guid.NewGuid());
        personDto.Address = null;

        // Act.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(personDto);

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void Validate_WhenAddressIsInvalid_ShouldHaveErrorFromAddressValidator()
    {
        // Arrange.
        // Verifica que el AddressValidator está enganchado (SetValidator): sus reglas se prueban en
        // AddressValidatorTests.
        Dto.Person personDto = PersonDtoMother.ValidPerson(Guid.NewGuid());
        personDto.Address = new Dto.Address { StreetLine1 = null };

        // Act.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(personDto);

        // Assert.
        result.ShouldHaveValidationErrorFor(x => x.Address!.StreetLine1);
    }

    [Fact]
    public void Validate_WhenSomePersonalAssetIsInvalid_ShouldHaveErrorFromPersonalAssetValidator()
    {
        // Arrange.
        // Verifica que el PersonalAssetValidator está enganchado para cada bien (RuleForEach): sus
        // reglas se prueban en PersonalAssetValidatorTests. El error lleva el índice del bien.
        Dto.Person personDto = PersonDtoMother.ValidPerson(
            Guid.NewGuid(),
            new List<Dto.PersonalAsset>
            {
                PersonDtoMother.PersonalAsset(),
                PersonDtoMother.PersonalAsset(value: 0M),
            });

        // Act.
        TestValidationResult<Dto.Person> result = _validator.TestValidate(personDto);

        // Assert.
        result.ShouldHaveValidationErrorFor("PersonalAssets[1].Value");
        result.ShouldNotHaveValidationErrorFor("PersonalAssets[0].Value");
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Arma un email válido del largo especificado, completando la parte local.
    /// </summary>
    /// <param name="length">El largo total del email.</param>
    /// <returns>El email.</returns>
    private static string BuildEmail(int length)
    {
        const string Domain = "@example.com";
        string email = new string('a', length - Domain.Length) + Domain;

        return email;
    }

    #endregion
}
