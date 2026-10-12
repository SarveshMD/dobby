namespace Dobby.Models;

class UrlItem
{
    Guid Id;
    string OriginalUrl { get; set; } = "";
    string ShortUrl { get; set; } = "";
    DateTimeOffset CreatedAt;

    private UrlItem() { }

    public UrlItem(string originalUrl, string shortUrl)
    {
        OriginalUrl = originalUrl;
        ShortUrl = shortUrl;
        CreatedAt = DateTime.UtcNow;
    }
}
