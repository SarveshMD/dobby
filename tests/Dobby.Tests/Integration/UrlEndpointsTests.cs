using System.Net;
using System.Net.Http.Json;
using Dobby.DTOs;
using FluentAssertions;

namespace Dobby.Tests.Integration;

public class UrlEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UrlEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // block HttpClient from following 302 redirects
            }
        );
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

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
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
