using Dobby.Validators;
using FluentValidation.TestHelper;
using Dobby.DTOs;

namespace Dobby.Tests.Validators;

public class LoginRequestDtoValidatorTests
{
    private readonly LoginRequestDtoValidator _validator = new();
    private readonly string _sampleValidUsername = "daemontargaryen";
    private readonly string _sampleValidPassword = "thisisavalidsamplepassword";

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("      ")]
    public void Validate_NullOrEmptyUsername_ShouldHaveValidationErrors(string? username)
    {
        // Arrange
        var invalidRegisterRequestDto = new LoginRequestDto(username!, _sampleValidPassword);

        // Act
        var validationResult = _validator.TestValidate(invalidRegisterRequestDto);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("taylorswift")]
    [InlineData("ViratKohli")]
    [InlineData("thor_odinson")]
    [InlineData("Tony_Stark_3000")]
    [InlineData("arya-stark-has-no-name")]
    [InlineData("this_is_exactly-30-characterss")]
    public void Validate_ValidUsername_ShouldNotHaveValidationErrors(string username)
    {
        // Arrange
        var validRegisterRequestDto = new LoginRequestDto(username!, _sampleValidPassword);

        // Act
        var validationResult = _validator.TestValidate(validRegisterRequestDto);

        // Assert
        validationResult.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("         ")]
    public void Validate_NullOrEmptyPassword_ShouldHaveValidationErrors(string? password)
    {
        // Arrange
        var invalidRegisterRequestDto = new LoginRequestDto(_sampleValidUsername, password!);

        // Act
        var validationResult = _validator.TestValidate(invalidRegisterRequestDto);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("Abc123!@")]
    [InlineData("hello world 42")]
    [InlineData("P@ssw0rd!2026")]
    [InlineData("My P@ss word")]
    [InlineData("spaces allowed")]
    [InlineData("128_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")]
    public void Validate_ValidPassword_ShouldNotHaveValidationErrors(string password)
    {
        // Arrange
        var validRegisterRequestDto = new LoginRequestDto(_sampleValidUsername, password);

        // Act
        var validationResult = _validator.TestValidate(validRegisterRequestDto);

        // Assert
        validationResult.ShouldNotHaveAnyValidationErrors();
    }
}
