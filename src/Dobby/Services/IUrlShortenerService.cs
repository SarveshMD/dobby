namespace Dobby.Services;

public interface IUrlShortenerService
{
    string Encode(int num);
    bool IsValidCode(string code);
    int Decode(string code);
}
