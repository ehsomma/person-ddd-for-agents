using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.ValueObjects;
using Xunit;

namespace BuildingBlocks.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests of the <see cref="Email"/> value object: required, from 1 to 362 characters, with the format
/// validated by a regex.
/// </summary>
public sealed class EmailTests
{
    #region Declarations

    // NOTE: Es un value object compartido: cada módulo que lo persiste tiene que tener una columna de
    // al menos este largo (en Persons, Persons.Email es varchar(362), RecordsPersons_Source.sql). Si
    // este test falla porque cambió el límite, revisá también esas columnas.
    private const int MaxLength = 362;

    #endregion

    #region Public methods

    [Theory]
    [InlineData("johndoe@gmail.com")]
    [InlineData("john.doe@mail.example.com")]
    [InlineData("john-doe@example.com")]
    public void Build_WhenValueIsValid_ShouldKeepValue(string value)
    {
        // Arrange.

        // Act.
        Email email = Email.Build(value);

        // Assert.
        email.Value.Should().Be(value);
    }

    [Fact]
    public void Build_WhenValueHasMaxLength_ShouldKeepValue()
    {
        // Arrange.
        // Se completa la parte local para que el email entero tenga el largo máximo.
        const string Domain = "@example.com";
        string value = new string('a', MaxLength - Domain.Length) + Domain;

        // Act.
        Email email = Email.Build(value);

        // Assert.
        email.Value.Should().Be(value);
    }

    [Fact]
    public void Build_WhenValueExceedsMaxLength_ShouldThrowDomainValidationException()
    {
        // Arrange.
        const string Domain = "@example.com";
        string value = new string('a', MaxLength + 1 - Domain.Length) + Domain;

        // Act.
        Action act = () => Email.Build(value);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Build_WhenValueIsNullOrEmpty_ShouldThrowDomainValidationException(string? value)
    {
        // Arrange.

        // Act.
        Action act = () => Email.Build(value);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Theory]
    [InlineData("johndoe")] // Sin @.
    [InlineData("@gmail.com")] // Sin parte local.
    [InlineData("john@doe@gmail.com")] // Dos @.
    [InlineData("johndoe@")] // Sin dominio.
    [InlineData("john doe@gmail.com")] // Con espacios.
    public void Build_WhenFormatIsInvalid_ShouldThrowDomainValidationException(string value)
    {
        // Arrange.

        // Act.
        Action act = () => Email.Build(value);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Build_WhenLocalPartHasPlusSign_ShouldThrowDomainValidationException()
    {
        // Arrange.
        // TODO: La regex solo acepta letras, números, "-" y "." en la parte local, así que rechaza
        // emails válidos como "john+news@gmail.com" (plus addressing) o "john_doe@gmail.com". Hay que
        // decidir si se amplía la regex; si se amplía, este test pasa a ser un caso válido.
        const string Value = "john+news@gmail.com";

        // Act.
        Action act = () => Email.Build(Value);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    #endregion
}
