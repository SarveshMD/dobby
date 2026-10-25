using FluentValidation;
using Dobby.DTOs;

namespace Dobby.Validators;

public class UrlIdDtoValidator : AbstractValidator<UrlIdDto>
{
    public UrlIdDtoValidator()
    {
        RuleFor(x => x.UrlId)
            .NotEmpty().WithMessage("UrlId cannot be empty")
            .Must(id =>
            {
                bool success = int.TryParse(id, out int intId);
                return id == id.Trim() && success && intId > 0;
            }).WithMessage("Invalid UrlId");
    }
}
