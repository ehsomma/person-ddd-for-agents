using AwesomeAssertions;
using BuildingBlocks.Domain.Exceptions;
using My.Exceptions;
using Xunit;

namespace BuildingBlocks.Domain.UnitTests.Exceptions;

/// <summary>
/// Tests of the <see cref="DomainValidationException"/>, thrown by the value objects and the
/// invariants of the aggregates.
/// </summary>
public sealed class DomainValidationExceptionTests
{
    #region Public methods

    [Fact]
    public void Constructor_WhenCalled_ShouldSetValidationCodeAndTypeInData()
    {
        // Arrange.

        // Act.
        DomainValidationException exception = new DomainValidationException("Value is invalid.");

        // Assert.
        // El código y el ErrorType los pone la base (ValidationException): con eso
        // GlobalExceptionHandler responde un 400 y no un 500.
        exception.Data[ExDataKey.ErrorCode].Should().Be(ExErrorCodeCore.ErrValidation);
        exception.Data[ExDataKey.ErrorType].Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Constructor_WhenInnerExceptionIsSpecified_ShouldKeepIt()
    {
        // Arrange.
        // Los value objects envuelven la excepción de la librería Throw: se conserva para el log.
        ArgumentOutOfRangeException innerException = new ArgumentOutOfRangeException("value");

        // Act.
        DomainValidationException exception = new DomainValidationException("Value is invalid.", innerException);

        // Assert.
        exception.InnerException.Should().BeSameAs(innerException);
        exception.Message.Should().Be("Value is invalid.");
    }

    #endregion
}
