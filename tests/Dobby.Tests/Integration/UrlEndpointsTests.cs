using System.Net;
using System.Net.Http.Json;
using Dobby.DTOs;
using FluentAssertions;

namespace Dobby.Tests.Integration;

public class UrlEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UrlEndpointsTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // block HttpClient from following 302 redirects
            }
        );
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task PostUrl_WithValidPayload_Returns201CreatedAndShortUrl()
    {
        // Arrange
        var payload = new CreateUrlItemDto("https://github.com");

        // Act
        var response = await _client.PostAsJsonAsync("/api/urls", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<ResponseUrlItemDto>();
        body.Should().NotBeNull();
        body.OriginalUrl.Should().Be(payload.OriginalUrl);
        body.UserId.Should().BeNull();
    }

    [Fact]
    public async Task GetUrl_WithInvalidCodeFormat_Returns400BadRequest()
    {
        // Arrange
        string invalidCode = "abc";

        // Act
        var response = await _client.GetAsync($"/{invalidCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUrl_WithValidExistingCode_Returns302Found()
    {
        // Arrange
        string targetUrl = "https://letterboxd.com/film/avengers-doomsday/";
        var createResponse = await _client.PostAsJsonAsync("/api/urls", new CreateUrlItemDto(targetUrl));
        createResponse.EnsureSuccessStatusCode();

        var createdItem = await createResponse.Content.ReadFromJsonAsync<ResponseUrlItemDto>();
        string shortCode = createdItem!.ShortUrl;

        // Act
        var redirectResponse = await _client.GetAsync($"/{shortCode}");

        // Assert
        redirectResponse.StatusCode.Should().Be(HttpStatusCode.Found); // 302
        redirectResponse.Headers.Location.Should().Be(new Uri(targetUrl));
    }
}
