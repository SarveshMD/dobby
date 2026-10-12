namespace Dobby.Services;


public class UrlShortener : IUrlShortenerService
{
    private readonly string _secretKey = "WHATEVER";

    public UrlShortener(string secretKey)
    {
        _secretKey = secretKey;
    }

    public string Encode(int num)
    {
        return "this must be the encoded string";
    }

    public int Decode(string code)
    {
        return 42;
    }
}
