
namespace Dobby.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public ICollection<UrlItem> UrlItems { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }

    private User() { }

    public User(string username, string passwordHash)
    {
        Id = Guid.NewGuid();
        Username = username;
        PasswordHash = passwordHash;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
