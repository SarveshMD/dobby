using Dobby.DTOs;
using FluentValidation;

namespace Dobby.Validators;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username cannot be empty")
            .MaximumLength(30).WithMessage("Username cannot be longer than 30 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .MaximumLength(128).WithMessage("Password cannot be longer than 128 characters");
    }
}

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username cannot be empty")
            .MaximumLength(30).WithMessage("Username cannot be longer than 30 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .MaximumLength(128).WithMessage("Password cannot be longer than 128 characters");
    }
}
