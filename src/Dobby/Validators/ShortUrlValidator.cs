using FluentValidation;
using Dobby.DTOs;
using Dobby.Services;

namespace Dobby.Validators;

public class ShortUrlValidator : AbstractValidator<GetShortUrlRequest>
{
    public ShortUrlValidator(IUrlShortenerService urlShortener)
    {
        RuleFor(x => x.ShortUrl)
            .NotEmpty().WithMessage("ShortUrl cannot be empty")
            .Length(6).WithMessage("ShortUrl must be exactly 6 characters long")
            .Matches("^[0-9a-zA-Z]+$").WithMessage("ShortUrl contains invalid Base62 characters")
            .Must(urlShortener.IsValidCode).WithMessage("ShortUrl is outside the supported range");
    }
}
