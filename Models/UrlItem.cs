namespace Dobby.Models;

public class UrlItem
{
    public Guid Id { get; set; }
    public string OriginalUrl { get; set; } = "";
    public string ShortUrl { get; set; } = "";
    public DateTimeOffset CreatedAt;

    private UrlItem() { }

    public UrlItem(string originalUrl, string shortUrl)
    {
        OriginalUrl = originalUrl;
        ShortUrl = shortUrl;
        CreatedAt = DateTime.UtcNow;
    }
}
