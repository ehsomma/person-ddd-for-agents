using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using Records.Persons.Domain.PersonAggregate.ValueObjects;
using Xunit;

namespace Records.Persons.Domain.UnitTests.PersonAggregate.ValueObjects;

/// <summary>
/// Tests del value object <see cref="StreetLine2"/>: opcional (acepta <see langword="null"/>), pero si
/// viene tiene que tener de 1 a 60 caracteres.
/// </summary>
public sealed class StreetLine2Tests
{
    #region Declarations

    // NOTE: Tiene que coincidir con el largo de la columna en la base (RecordsPersons_Source.sql).
    // Si este test falla porque cambió el límite, revisá también la columna.
    private const int MaxLength = 60;

    #endregion

    #region Public methods

    [Fact]
    public void Build_WhenValueIsNull_ShouldHaveNoValue()
    {
        // Arrange.

        // Act.
        StreetLine2 streetLine2 = StreetLine2.Build(null);

        // Assert.
        streetLine2.Value.Should().BeNull();
        streetLine2.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Build_WhenValueHasMaxLength_ShouldKeepValue()
    {
        // Arrange.
        string value = new string('a', MaxLength);

        // Act.
        StreetLine2 streetLine2 = StreetLine2.Build(value);

        // Assert.
        streetLine2.Value.Should().Be(value);
    }

    [Fact]
    public void Build_WhenValueIsEmpty_ShouldThrowDomainValidationException()
    {
        // Arrange.

        // Act.
        // Opcional no quiere decir que acepte vacío: si no hay dato, tiene que venir null.
        Action act = () => StreetLine2.Build(string.Empty);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Build_WhenValueExceedsMaxLength_ShouldThrowDomainValidationException()
    {
        // Arrange.
        string value = new string('a', MaxLength + 1);

        // Act.
        Action act = () => StreetLine2.Build(value);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    #endregion
}
