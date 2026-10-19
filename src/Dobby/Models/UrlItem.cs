namespace Dobby.Models;

public class UrlItem
{
    public int Id { get; private set; }
    public string OriginalUrl { get; private set; } = "";
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? UserId { get; private set; } = null;
    public User? User { get; private set; } = null;
    private UrlItem() { }

    public UrlItem(string originalUrl, Guid? userId = null)
    {
        OriginalUrl = originalUrl;
        CreatedAt = DateTimeOffset.UtcNow;
        UserId = userId;
    }
}
