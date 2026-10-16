using Dobby.Models;

namespace Dobby.DTOs;

public record CreateUrlItemDto(
    string OriginalUrl
);

public record ResponseUrlItemDto(
    int Id,
    string OriginalUrl,
    string ShortUrl
)
{
    public static ResponseUrlItemDto FromEntityAndShortUrl(UrlItem item, string shortCode)
    {
        return new ResponseUrlItemDto(
            item.Id,
            item.OriginalUrl,
            ShortUrl: shortCode
        );
    }
};

public record GetShortUrlRequest(string ShortUrl);
