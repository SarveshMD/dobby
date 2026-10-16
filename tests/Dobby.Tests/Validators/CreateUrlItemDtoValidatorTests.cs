
using Dobby.Validators;
using FluentValidation.TestHelper;
using FluentAssertions;
using Dobby.DTOs;

namespace Dobby.Tests.Validators;

public class CreateUrlItemDtoValidatorTests
{
    private readonly CreateUrlItemDtoValidator _validator = new();

    [Theory]
    [InlineData("https://google.com")]
    [InlineData("https://localhost:8080/api/items")]
    [InlineData("https://www.example.com/api/v1/users/123456789/profile/settings/notifications/email/preferences?username=sarvesh123&redirect=https%3A%2F%2Fexample.com%2Fdashboard&session_id=abcdef1234567890&include_metadata=true&sort_by=created_at&sort_order=descending&page=42&page_size=100&filter_status=active&search_query=computer+science+internship+preparation&category=technology&subcategory=software_engineering&language=en-IN&timezone=Asia%2FKolkata&client_version=2.5.7&device_type=desktop&tracking_id=trk_98765432109876543210&experiment_group=variant_b&callback_url=https%3A%2F%2Fexample.com%2Fauth%2Fcallback%3Fsource%3Demail&expires_at=2027-12-31T23%3A59%3A59Z&signature=a1b2c3d4e5f6g7h8i9j0&debug=false")]
    public void Validate_ValidUrl_ShouldNotHaveErrors(string validUrl)
    {
        // Arrange
        var validUrlDto = new CreateUrlItemDto(validUrl);

        // Act
        var validationResult = _validator.TestValidate(validUrlDto);

        // Assert
        validationResult.ShouldNotHaveValidationErrorFor(x => x.OriginalUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_NullOrEmptyUrl_ShouldHaveValidationError(string? invalidUrl)
    {
        // Arrange
        var invalidUrlDto = new CreateUrlItemDto(invalidUrl!);

        // Act
        var validationResult = _validator.TestValidate(invalidUrlDto);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.OriginalUrl);
    }
}
