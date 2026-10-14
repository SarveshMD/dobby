namespace Dobby.Models;

public class UrlItem
{
    public int Id { get; private set; }
    public string OriginalUrl { get; private set; } = "";
    public string ShortUrl { get; private set; } = "";
    public DateTimeOffset CreatedAt { get; private set; }

    private UrlItem() { }

    public UrlItem(int id, string originalUrl, string shortUrl)
    {
        Id = id;
        OriginalUrl = originalUrl;
        ShortUrl = shortUrl;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
