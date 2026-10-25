using FluentValidation;
using Dobby.DTOs;

namespace Dobby.Validators;

public class UrlIdValidator : AbstractValidator<UrlIdDto>
{
    public UrlIdValidator()
    {
        RuleFor(x => x.UrlId)
            .NotNull().WithMessage("UrlId cannot be null")
            .NotEmpty().WithMessage("UrlId cannot be empty")
            .Must(id =>
            {
                bool success = int.TryParse(id, out int intId);
                return success && intId > 0;
            }).WithMessage("Invalid UrlId");
    }
}
