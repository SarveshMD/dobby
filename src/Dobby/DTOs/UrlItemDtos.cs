using Dobby.Models;

namespace Dobby.DTOs;

public record CreateUrlItemDto(
    string OriginalUrl
);

public record ResponseUrlItemDto(
    int Id,
    string OriginalUrl,
    string ShortUrl,
    Guid? UserId
)
{
    public static ResponseUrlItemDto FromEntityAndShortUrl(UrlItem item, string shortCode)
    {
        return new ResponseUrlItemDto(
            Id: item.Id,
            OriginalUrl: item.OriginalUrl,
            ShortUrl: shortCode,
            UserId: item.UserId
        );
    }
};

public record GetShortUrlRequest(string ShortUrl);
