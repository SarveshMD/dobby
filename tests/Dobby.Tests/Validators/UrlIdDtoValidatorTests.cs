using Dobby.Validators;
using Dobby.DTOs;

using FluentValidation.TestHelper;

namespace Dobby.Tests.Validators;

public class UrlIdDtoValidatorTests
{
    private readonly UrlIdDtoValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("       ")]
    public void Validate_NullOrEmptyOrWhitespaceUrlId_ShouldHaveValidationError(string? invalidUrlId)
    {
        // Arrange
        var invalidUrlIdDto = new UrlIdDto(invalidUrlId!);

        // Act
        var validationResult = _validator.TestValidate(invalidUrlIdDto);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.UrlId);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("-10")]
    [InlineData("-0")]
    public void Validate_BadNumbers_ShouldHaveValidationError(string invalidUrlId)
    {
        // Arrange
        var invalidUrlIdDto = new UrlIdDto(invalidUrlId);

        // Act
        var validationResult = _validator.TestValidate(invalidUrlIdDto);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.UrlId);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("1280")]
    [InlineData("1989")]
    [InlineData("999")]
    [InlineData("2147483647")] // int.MaxValue
    public void Validate_ValidNumbers_ShouldNotHaveValidationError(string validUrlId)
    {
        // Arrange
        var validUrlIdDto = new UrlIdDto(validUrlId);

        // Act
        var validationResult = _validator.TestValidate(validUrlIdDto);

        // Assert
        validationResult.ShouldNotHaveAnyValidationErrors();
    }

}
