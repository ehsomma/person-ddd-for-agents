using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.UnitTests.TestData;
using Xunit;

namespace BuildingBlocks.Domain.UnitTests.Models;

/// <summary>
/// Tests of the <see cref="Enumeration{TEnum}"/> base class, using the <see cref="TestColor"/>
/// enumeration.
/// </summary>
public sealed class EnumerationTests
{
    #region Public methods

    [Fact]
    public void List_WhenCalled_ShouldReturnAllDeclaredOptions()
    {
        // Arrange.

        // Act.
        // List se arma por reflection con los campos public static del tipo: si una opción nueva no
        // aparece, no está declarada como public static readonly.
        IReadOnlyCollection<TestColor> colors = TestColor.List;

        // Assert.
        colors.Should().BeEquivalentTo(new TestColor[] { TestColor.Red, TestColor.Green, TestColor.Blue });
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(4, false)]
    public void ContainsValue_WhenCalled_ShouldReturnWhetherTheValueExists(int value, bool expected)
    {
        // Arrange.

        // Act.
        bool containsValue = TestColor.ContainsValue(value);

        // Assert.
        containsValue.Should().Be(expected);
    }

    [Fact]
    public void FromValue_WhenValueIsValid_ShouldReturnTheSameInstance()
    {
        // Arrange.

        // Act.
        TestColor color = TestColor.FromValue(2);

        // Assert.
        // Las opciones son singletons: FromValue devuelve la instancia declarada, no una copia.
        color.Should().BeSameAs(TestColor.Green);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Yellow")]
    public void FromName_WhenNameIsInvalid_ShouldThrowDomainValidationException(string? name)
    {
        // Arrange.

        // Act.
        Action act = () => TestColor.FromName(name);

        // Assert.
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Equals_WhenValuesAreEqual_ShouldBeEqual()
    {
        // Arrange.
        TestColor color = TestColor.FromName("red");

        // Act.
        bool areEqual = color == TestColor.Red;

        // Assert.
        areEqual.Should().BeTrue();
    }

    [Fact]
    public void CompareTo_WhenCalled_ShouldCompareByValue()
    {
        // Arrange.

        // Act.
        int comparison = TestColor.Red.CompareTo(TestColor.Blue);

        // Assert.
        // Red (1) va antes que Blue (3): se ordena por el valor, no por el nombre.
        comparison.Should().BeNegative();
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnName()
    {
        // Arrange.

        // Act.
        string value = TestColor.Green.ToString();

        // Assert.
        value.Should().Be("Green");
    }

    #endregion
}
