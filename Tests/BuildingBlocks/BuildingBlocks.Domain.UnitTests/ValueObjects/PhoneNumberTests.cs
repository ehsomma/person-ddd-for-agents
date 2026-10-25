using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.ValueObjects;
using Xunit;

namespace BuildingBlocks.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests of the <see cref="PhoneNumber"/> value object: optional (accepts <see langword="null"/> and
/// empty), up to 60 characters.
/// </summary>
public sealed class PhoneNumberTests
{
    #region Declarations

    // TODO: El value object acepta hasta 60 caracteres, pero en Persons la columna Persons.Phone es
    // nvarchar(20) (RecordsPersons_Source.sql): un teléfono de 21 a 60 caracteres pasa el dominio y
    // falla al insertar. Hay que decidir cuál es el límite correcto y alinear el value object, la
    // columna y este test.
    private const int MaxLength = 60;

    #endregion

    #region Public methods

    [Fact]
    public void Build_WhenValueIsValid_ShouldKeepValue()
    {
        // Arrange.

        // Act.
        PhoneNumber phoneNumber = PhoneNumber.Build("+54 11 4444-5555");

        // Assert.
        phoneNumber.Value.Should().Be("+54 11 4444-5555");
        phoneNumber.HasValue.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Build_WhenValueIsNullOrEmpty_ShouldHaveNoValue(string? value)
    {
        // Arrange.

        // Act.
        PhoneNumber phoneNumber = PhoneNumber.Build(value);

        // Assert.
        phoneNumber.HasValue.Should().BeFalse();
        phoneNumber.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Build_WhenValueHasMaxLength_ShouldKeepValue()
    {
        // Arrange.
        string value = new string('1', MaxLength);

        // Act.
        PhoneNumber phoneNumber = PhoneNumber.Build(value);

        // Assert.
        phoneNumber.Value.Should().Be(value);
    }

    [Fact]
    public void Build_WhenValueExceedsMaxLength_ShouldThrowDomainValidationException()
    {
        // Arrange.
        string value = new string('1', MaxLength + 1);

        // Act.
        Action act = () => PhoneNumber.Build(value);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void ImplicitOperator_WhenValueObjectIsNull_ShouldReturnNull()
    {
        // Arrange.
        PhoneNumber? phoneNumber = null;

        // Act.
        // A diferencia de los value objects obligatorios (StringValueObject), la conversión a string
        // de uno opcional acepta null: un teléfono que no vino se mapea a null sin chequearlo antes.
        string? value = phoneNumber;

        // Assert.
        value.Should().BeNull();
    }

    #endregion
}
