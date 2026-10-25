using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.ValueObjects;
using Xunit;

namespace BuildingBlocks.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests of the <see cref="PhoneNumber"/> value object: optional (accepts <see langword="null"/> and
/// empty), up to 30 characters.
/// </summary>
public sealed class PhoneNumberTests
{
    #region Declarations

    // NOTE: Es un value object compartido: cada módulo que lo persiste tiene que tener una columna de
    // al menos este largo (en Persons, Persons.Phone es nvarchar(30), RecordsPersons_Source.sql). 30
    // alcanza para un número con formato y extensión ("+54 11 4444-5555 int. 1234"). Si este test
    // falla porque cambió el límite, revisá también esas columnas y el PersonValidator de la API.
    private const int MaxLength = 30;

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
