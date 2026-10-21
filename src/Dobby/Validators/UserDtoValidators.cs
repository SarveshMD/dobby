using Dobby.DTOs;
using FluentValidation;

namespace Dobby.Validators;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username cannot be empty")
            .Matches(@"^[a-zA-Z0-9_-]{3,30}$").WithMessage("Username can only contain letters, numbers, underscores or hyphens, and must be between 3 and 30 characters in length");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password cannot be empty")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .MaximumLength(128).WithMessage("Password cannot be longer than 128 characters");
    }
}

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username cannot be empty");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password cannot be empty");
    }
}
