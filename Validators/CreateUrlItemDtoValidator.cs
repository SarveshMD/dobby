using Dobby.DTOs;
using FluentValidation;

namespace Dobby.Validators;

public class CreateUrlItemDtoValidator : AbstractValidator<CreateUrlItemDto>
{
    public CreateUrlItemDtoValidator()
    {
        RuleFor(item => item.OriginalUrl)
            .NotEmpty().WithMessage("URL cannot be empty")
            .MaximumLength(2048).WithMessage("URL cannot be longer than 2048 characters");
    }
}
