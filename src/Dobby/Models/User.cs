
namespace Dobby.Models;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = "";
    public string PasswordHash { get; private set; } = "";
    public DateTimeOffset CreatedAt { get; private set; }
    public ICollection<UrlItem> UrlItems { get; private set; } = [];

    private User() { }

    public User(string username, string passwordHash)
    {
        Id = Guid.NewGuid();
        Username = username;
        PasswordHash = passwordHash;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
