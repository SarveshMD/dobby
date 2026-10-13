using System.Text;
using Eleven41.Skip32;

namespace Dobby.Services;


public class UrlShortener : IUrlShortenerService
{
    private readonly string _secretKey = "WHATEVER42";
    private readonly byte[] _secretKeyBytes = [];
    private readonly Skip32Cipher _skip32 = null!;
    private readonly string _sixtyTwoBytes = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public UrlShortener(string secretKey)
    {
        _secretKey = secretKey;
        _secretKeyBytes = Encoding.UTF8.GetBytes(_secretKey);
        _skip32 = new Skip32Cipher(_secretKeyBytes);
    }

    public string Encode(int num)
    {
        uint value = (uint)_skip32.Encrypt(num);

        StringBuilder sb = new StringBuilder();

        while (value > 0)
        {
            int mod = (int)(value % 62);
            char currChar = _sixtyTwoBytes[mod];
            sb.Append(currChar);
            value /= 62;
        }

        string res = new string(sb.ToString().PadRight(6, '0').Reverse().ToArray());

        return res;
    }

    public int Decode(string code)
    {
        int scrambledValue = int.Parse(code);
        return _skip32.Decrypt(scrambledValue);
    }
}
