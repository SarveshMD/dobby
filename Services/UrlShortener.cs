using System.Numerics;
using System.Security.Cryptography.Xml;
using System.Text;
using Eleven41.Skip32;

namespace Dobby.Services;


public class UrlShortener : IUrlShortenerService
{
    private readonly Skip32Cipher _skip32 = null!;
    private readonly string _sixtyTwoBytes = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private readonly int[] _lookupTable = new int[128];

    public UrlShortener(string secretKey)
    {
        _skip32 = new Skip32Cipher(Encoding.UTF8.GetBytes(secretKey));

        Array.Fill(_lookupTable, -1);

        for (int i = 0; i < 62; i++)
        {
            _lookupTable[_sixtyTwoBytes[i]] = i;
        }
    }

    public string Encode(int num)
    {
        uint value = (uint)_skip32.Encrypt(num);

        char[] res = new char[6];

        int idx = 5;
        while (value > 0)
        {
            int mod = (int)(value % 62);
            char currChar = _sixtyTwoBytes[mod];
            res[idx] = currChar;
            idx--;
            value /= 62;
        }

        while (idx >= 0)
        {
            res[idx] = '0';
            idx--;
        }

        return new string(res);
    }

    public int Decode(string code)
    {
        uint scrambled = 0;

        foreach (char c in code)
        {
            scrambled = (scrambled * 62) + (uint)_lookupTable[c];
        }

        int encrypted = unchecked((int)scrambled);
        int res = _skip32.Decrypt(encrypted);

        return res;
    }
}
