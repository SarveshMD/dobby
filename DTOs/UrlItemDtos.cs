namespace Dobby.DTOs;

public record CreateUrlItemDto(
    string OriginalUrl
);

public record ResponseUrlItemDto(
    Guid Id,
    string OriginalUrl,
    string ShortUrl,
    DateTimeOffset CreatedAt
);
