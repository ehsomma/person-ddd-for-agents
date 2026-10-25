using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.ValueObjects;
using Xunit;

namespace BuildingBlocks.Domain.UnitTests.Models;

/// <summary>
/// Tests of the equality of the <see cref="ValueObject"/> base class (by value, not by reference),
/// using real value objects of the building blocks.
/// </summary>
public sealed class ValueObjectTests
{
    #region Public methods

    [Fact]
    public void Equals_WhenValuesAreEqual_ShouldBeEqualAndHaveSameHashCode()
    {
        // Arrange.
        Email email = Email.Build("johndoe@gmail.com");
        Email otherEmail = Email.Build("johndoe@gmail.com");

        // Act.
        bool areEqual = email.Equals(otherEmail);

        // Assert.
        // Dos objetos iguales tienen que tener el mismo hash code: si no, un Dictionary o un HashSet
        // los trataría como distintos.
        areEqual.Should().BeTrue();
        email.GetHashCode().Should().Be(otherEmail.GetHashCode());
    }

    [Fact]
    public void Equals_WhenValuesAreDifferent_ShouldNotBeEqual()
    {
        // Arrange.
        Email email = Email.Build("johndoe@gmail.com");
        Email otherEmail = Email.Build("janeroe@gmail.com");

        // Act.
        bool areEqual = email == otherEmail;

        // Assert.
        areEqual.Should().BeFalse();
    }

    [Fact]
    [SuppressMessage(
        "Maintainability",
        "CA1508:Avoid dead conditional code",
        Justification = "Se testea la sobrecarga del operador == con null: el analyzer asume el == estándar.")]
    public void EqualityOperator_WhenBothAreNull_ShouldBeEqual()
    {
        // Arrange.
        Email? email = null;
        Email? otherEmail = null;

        // Act.
        bool areEqual = email == otherEmail;

        // Assert.
        areEqual.Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WhenOnlyOneIsNull_ShouldNotBeEqual()
    {
        // Arrange.
        Email email = Email.Build("johndoe@gmail.com");
        Email? otherEmail = null;

        // Act.
        bool areEqual = email == otherEmail;

        // Assert.
        areEqual.Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenTypesAreDifferentWithSameValue_ShouldBeEqual()
    {
        // Arrange.
        // TODO: ValueObject compara solo los componentes de igualdad, no el tipo: un Email y un
        // PhoneNumber con el mismo texto son "iguales". Lo mismo pasa con City y State en Persons
        // (los dos son strings). Hoy no rompe nada porque no se comparan value objects de tipos
        // distintos, pero no es lo esperado. Hay que decidir si ValuesEqual agrega
        // GetType() == other.GetType() (como hace Enumeration); si se agrega, este test pasa a
        // esperar que no sean iguales.
        Email email = Email.Build("johndoe@gmail.com");
        PhoneNumber phoneNumber = PhoneNumber.Build("johndoe@gmail.com");

        // Act.
        bool areEqual = email.Equals(phoneNumber);

        // Assert.
        areEqual.Should().BeTrue();
    }

    #endregion
}
