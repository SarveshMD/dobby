namespace Dobby.Services;

public interface IUrlShortenerService
{
    string Encode(int num);
    int Decode(string code);
}
