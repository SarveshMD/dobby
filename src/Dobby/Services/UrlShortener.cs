using System.Text;
using Eleven41.Skip32;

namespace Dobby.Services;


public class UrlShortener : IUrlShortenerService
{
    private readonly Skip32Cipher _skip32 = null!;
    private const string SixtyTwoChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private readonly static int[] LookupTable = new int[128];

    public UrlShortener(string secretKey)
    {
        _skip32 = new Skip32Cipher(Encoding.UTF8.GetBytes(secretKey));

        Array.Fill(LookupTable, -1);

        for (int i = 0; i < 62; i++)
        {
            LookupTable[SixtyTwoChars[i]] = i;
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
            char currChar = SixtyTwoChars[mod];
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
        if (code is null || code.Length != 6)
        {
            throw new ArgumentException($"Short Code must be exactly 6 characters.", nameof(code));
        }

        uint scrambled = 0;

        foreach (char c in code)
        {
            if (c >= LookupTable.Length || LookupTable[c] == -1)
            {
                throw new ArgumentException($"Invalid character '{c}' in Short Code.", nameof(code));
            }

            scrambled = (scrambled * 62) + (uint)LookupTable[c];
        }

        int encrypted = unchecked((int)scrambled);
        int res = _skip32.Decrypt(encrypted);

        return res;
    }
}
