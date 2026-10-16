using Dobby.Validators;
using FluentValidation.TestHelper;
using Dobby.DTOs;

namespace Dobby.Tests.Validators;

public class ShortUrlValidatorTests
{
    private readonly ShortUrlValidator _validator = new();

    [Theory]
    [InlineData("aB3k9Z")]
    [InlineData("000000")]
    [InlineData("ZZZZZZ")]
    public void Validate_ValidShortUrl_ShouldNotHaveErrors(string validShortUrl)
    {
        // Arrange
        var validShortUrlDto = new GetShortUrlRequest(validShortUrl);

        // Act
        var validationResult = _validator.TestValidate(validShortUrlDto);

        // Assert
        validationResult
            .ShouldNotHaveValidationErrorFor(x => x.ShortUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("    ")]
    [InlineData("      ")]
    public void Validate_NullOrEmptyUrl_ShouldHaveValidationError(string? invalidShortUrl)
    {
        // Arrange
        var invalidShortUrlDto = new GetShortUrlRequest(invalidShortUrl!);

        // Act
        var validationResult = _validator.TestValidate(invalidShortUrlDto);

        // Assert
        validationResult
            .ShouldHaveValidationErrorFor(x => x.ShortUrl);
    }

    [Theory]
    [InlineData("notsixcharacters")]
    [InlineData("5char")]
    [InlineData("abcd")]
    [InlineData("abcdefg")]
    public void Validate_LengthNotSixShortUrl_ShouldHaveValidationError(string invalidShortUrl)
    {
        // Arrange
        var invalidShortUrlDto = new GetShortUrlRequest(invalidShortUrl);

        // Act
        var validationResult = _validator.TestValidate(invalidShortUrlDto);

        // Assert
        validationResult
            .ShouldHaveValidationErrorFor(x => x.ShortUrl)
            .WithErrorMessage("ShortUrl must be exactly 6 characters long");
    }

    [Theory]
    [InlineData("💀😭🤦")]
    [InlineData("a💀🤦z")]
    [InlineData("abc_xy")]
    [InlineData("abc xy")]
    [InlineData("abc!xy")]
    [InlineData("abc-xy")]
    [InlineData("a$b#c&")]
    [InlineData("      ")]
    public void Validate_NotBase62ShortUrl_ShouldHaveValidationErrors(string invalidShortUrl)
    {
        // Arrange
        var invalidShortUrlDto = new GetShortUrlRequest(invalidShortUrl);

        // Act
        var validationResult = _validator.TestValidate(invalidShortUrlDto);

        // Assert
        validationResult
            .ShouldHaveValidationErrorFor(x => x.ShortUrl)
            .WithErrorMessage("ShortUrl contains invalid Base62 characters");
    }
}
