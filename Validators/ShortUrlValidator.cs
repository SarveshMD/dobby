using FluentValidation;
using Dobby.DTOs;

public class ShortUrlValidator : AbstractValidator<GetShortUrlRequest>
{
    public ShortUrlValidator()
    {
        RuleFor(x => x.ShortUrl)
            .NotEmpty().WithMessage("ShortUrl cannot be empty")
            .Length(6).WithMessage("ShortUrl must be exactly 6 characters long")
            .Matches("^[0-9a-zA-z]+$").WithMessage("ShortUrl contains invalid Base62 characters");
    }
}
