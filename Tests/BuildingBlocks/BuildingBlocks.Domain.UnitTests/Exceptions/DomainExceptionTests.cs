using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.Models;
using My.Exceptions;
using Xunit;

namespace BuildingBlocks.Domain.UnitTests.Exceptions;

/// <summary>
/// Tests of the <see cref="DomainException"/>: it carries the <see cref="Error"/> and copies its code
/// and type to <see cref="Exception.Data"/>.
/// </summary>
public sealed class DomainExceptionTests
{
    #region Public methods

    [Fact]
    public void Constructor_WhenErrorIsSpecified_ShouldKeepErrorAndUseItsMessage()
    {
        // Arrange.
        Error error = new Error("ERR.TEST.NOTFOUND", "Test not found.", ErrorType.NotFound);

        // Act.
        DomainException exception = new DomainException(error);

        // Assert.
        exception.Error.Should().BeSameAs(error);
        exception.Message.Should().Be("Test not found.");
    }

    [Fact]
    public void Constructor_WhenErrorIsSpecified_ShouldSetErrorCodeAndTypeInData()
    {
        // Arrange.
        Error error = new Error("ERR.TEST.NOTFOUND", "Test not found.", ErrorType.NotFound);

        // Act.
        DomainException exception = new DomainException(error);

        // Assert.
        // GlobalExceptionHandler no conoce DomainException: lee el código y el ErrorType de Data
        // para armar el ErrorResponse y elegir el status HTTP. Si no estuvieran, el error saldría
        // como un 500 genérico.
        exception.Data[ExDataKey.ErrorCode].Should().Be("ERR.TEST.NOTFOUND");
        exception.Data[ExDataKey.ErrorType].Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Constructor_WhenErrorIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange.

        // Act.
        Action act = () => _ = new DomainException(null!);

        // Assert.
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("error");
    }

    #endregion
}
