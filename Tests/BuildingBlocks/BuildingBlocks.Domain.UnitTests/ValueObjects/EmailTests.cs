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
    [InlineData("john+news@gmail.com")] // Plus addressing de Gmail.
    [InlineData("john_doe@gmail.com")]
    [InlineData("o'brien@gmail.com")]
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
    [InlineData("johndoe@gmail")] // Sin punto en el dominio.
    [InlineData("johndoe@gmail..com")] // Punto vacío en el dominio.
    [InlineData("johndoe@gmail.com con texto")] // Texto de más después del dominio.
    public void Build_WhenFormatIsInvalid_ShouldThrowDomainValidationException(string value)
    {
        // Arrange.

        // Act.
        Action act = () => Email.Build(value);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    #endregion
}
