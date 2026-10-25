using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using Records.Persons.Domain.PersonAggregate.ValueObjects;
using Xunit;

namespace Records.Persons.Domain.UnitTests.PersonAggregate.ValueObjects;

/// <summary>
/// Tests del value object <see cref="State"/>: opcional (acepta <see langword="null"/>), pero si viene
/// tiene que tener de 1 a 50 caracteres.
/// </summary>
public sealed class StateTests
{
    #region Declarations

    // NOTE: Tiene que coincidir con el largo de la columna en la base (RecordsPersons_Source.sql).
    // Si este test falla porque cambió el límite, revisá también la columna.
    private const int MaxLength = 50;

    #endregion

    #region Public methods

    [Fact]
    public void Build_WhenValueIsNull_ShouldHaveNoValue()
    {
        // Arrange.

        // Act.
        State state = State.Build(null);

        // Assert.
        state.Value.Should().BeNull();
        state.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Build_WhenValueHasMaxLength_ShouldKeepValue()
    {
        // Arrange.
        string value = new string('a', MaxLength);

        // Act.
        State state = State.Build(value);

        // Assert.
        state.Value.Should().Be(value);
    }

    [Fact]
    public void Build_WhenValueIsEmpty_ShouldThrowDomainValidationException()
    {
        // Arrange.

        // Act.
        Action act = () => State.Build(string.Empty);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Build_WhenValueExceedsMaxLength_ShouldThrowDomainValidationException()
    {
        // Arrange.
        string value = new string('a', MaxLength + 1);

        // Act.
        Action act = () => State.Build(value);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    #endregion
}
