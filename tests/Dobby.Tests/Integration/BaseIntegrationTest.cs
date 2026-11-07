using Dobby.Services;
using Dobby.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;

public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory _factory;
    protected readonly HttpClient _client;

    public BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // block HttpClient from following 302 redirects
            }
        );
    }

    protected ITokenService TokenService =>
        _factory.Services.GetRequiredService<ITokenService>();
}
