using Dobby.Models;

namespace Dobby.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}
