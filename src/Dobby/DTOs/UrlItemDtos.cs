using Dobby.Models;

namespace Dobby.DTOs;

public record CreateUrlItemDto(
    string OriginalUrl
);

public record ResponseUrlItemDto(
    int Id,
    string OriginalUrl
)
{
    public static ResponseUrlItemDto FromEntity(UrlItem item)
    {
        return new ResponseUrlItemDto(
            item.Id,
            item.OriginalUrl
        );
    }
};

public record GetShortUrlRequest(string ShortUrl);
