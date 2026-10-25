using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dobby.DTOs;
using FluentAssertions;

namespace Dobby.Tests.Integration;

public class UrlEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly string _validUsername = "ValdiUsername";
    private readonly string _validPassword = "ValidPassword";

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

    private async Task<(string Username, string Password, AuthResponseDto body)>
        CreateRegisteredUserAsync()
    {
        var username = _validUsername;
        var password = _validPassword;

        var payload = new RegisterRequestDto(username, password);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            payload
        );

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body.Should().NotBeNull();

        return (username, password, body);
    }

    private async Task<ResponseUrlItemDto> CreateUrlItemAsync(string url, string? token = null)
    {
        var createUrlItem = new CreateUrlItemDto(url);

        var postUrlItemRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/urls"
        )
        {
            Content = JsonContent.Create(createUrlItem)
        };
        postUrlItemRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(postUrlItemRequest);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseUrlItemDto = await response.Content.ReadFromJsonAsync<ResponseUrlItemDto>();
        responseUrlItemDto.Should().NotBeNull();

        return responseUrlItemDto;
    }

    [Fact]
    public async Task PostUrl_WithValidPayload_Returns201CreatedAndResponseDto()
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

    [Fact]
    public async Task PostUrl_WithValidPayloadAndAuthorization_Returns201CreatedAndResponseDtoWithUserId()
    {
        // Arrange
        var (_, _, userBody) = await CreateRegisteredUserAsync();
        string url = "https://letterboxd.com/film/premalu/";

        // Act
        var responseUrlItemDto = await CreateUrlItemAsync(url, userBody.Token);

        // Assert
        responseUrlItemDto.OriginalUrl.Should().Be(url);
        responseUrlItemDto.UserId.Should().Be(userBody.Id);
    }
}
