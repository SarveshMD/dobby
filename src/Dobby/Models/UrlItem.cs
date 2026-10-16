namespace Dobby.Models;

public class UrlItem
{
    public int Id { get; private set; }
    public string OriginalUrl { get; private set; } = "";
    public DateTimeOffset CreatedAt { get; private set; }

    private UrlItem() { }

    public UrlItem(string originalUrl)
    {
        OriginalUrl = originalUrl;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
