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
    public static ResponseUrlItemDto FromEntity(UrlItem item)
    {
        return new ResponseUrlItemDto(
            item.Id,
            item.OriginalUrl,
            item.ShortUrl
        );
    }
};
