using Dobby.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Dobby.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["UrlShortener:SecretKey"] = "TestSecret",
                ["Jwt:Issuer"] = "DobbyBackend",
                ["Jwt:Audience"] = "DobbyClient",
                ["Jwt:Key"] = "A_VERY_LONG_SECRET_KEY_THAT_IS_AT_LEAST_32_BYTES_LONG_FOR_SECURITY",
                ["Jwt:ExpiryMinutes"] = "60",
                ["Redis:ConnectionString"] = "localhost:6379"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.AddDbContext<UrlDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });
        });

    }
}
