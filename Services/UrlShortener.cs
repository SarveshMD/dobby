using System.Text;
using Eleven41.Skip32;

namespace Dobby.Services;


public class UrlShortener : IUrlShortenerService
{
    private readonly string _secretKey = "WHATEVER42";
    private readonly byte[] _secretKeyBytes = [];
    private readonly Skip32Cipher _skip32 = null!;

    public UrlShortener(string secretKey)
    {
        _secretKey = secretKey;
        _secretKeyBytes = Encoding.UTF8.GetBytes(_secretKey);
        _skip32 = new Skip32Cipher(_secretKeyBytes);
    }

    public string Encode(int num)
    {

        int scrambledValue = _skip32.Encrypt(num);
        return scrambledValue.ToString();
    }

    public int Decode(string code)
    {
        int scrambledValue = int.Parse(code);
        return _skip32.Decrypt(scrambledValue);
    }
}
