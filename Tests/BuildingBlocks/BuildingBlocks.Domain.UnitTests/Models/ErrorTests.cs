using AwesomeAssertions;
using BuildingBlocks.Domain.Models;
using My.Exceptions;
using Xunit;

namespace BuildingBlocks.Domain.UnitTests.Models;

/// <summary>
/// Tests of the domain <see cref="Error"/>: code (required), message and <see cref="ErrorType"/>.
/// </summary>
public sealed class ErrorTests
{
    #region Public methods

    [Fact]
    public void Constructor_WhenDataIsValid_ShouldSetAllProperties()
    {
        // Arrange.

        // Act.
        Error error = new Error("ERR.TEST.NOTFOUND", "Test not found.", ErrorType.NotFound);

        // Assert.
        error.Code.Should().Be("ERR.TEST.NOTFOUND");
        error.Message.Should().Be("Test not found.");
        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WhenCodeIsNullOrWhiteSpace_ShouldThrowArgumentException(string? invalidCode)
    {
        // Arrange.

        // Act.
        // El código es el contrato del error (viaja en el ErrorResponse), así que no puede faltar.
        Action act = () => _ = new Error(invalidCode!, "Test not found.", ErrorType.NotFound);

        // Assert.
        // ArgumentNullException (para null) hereda de ArgumentException, así que este Throw cubre
        // los tres casos.
        act.Should().Throw<ArgumentException>()
            .WithParameterName("code");
    }

    [Fact]
    public void ImplicitOperator_WhenCalled_ShouldReturnCode()
    {
        // Arrange.
        Error error = new Error("ERR.TEST.NOTFOUND", "Test not found.", ErrorType.NotFound);

        // Act.
        string value = error;

        // Assert.
        value.Should().Be("ERR.TEST.NOTFOUND");
    }

    #endregion
}
