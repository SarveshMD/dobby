using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dobby.DTOs;
using FluentAssertions;

namespace Dobby.Tests.Integration;

public static class IntegrationTestHelpers
{
    private static readonly string _registerEndpoint = "/api/auth/register";
    private static readonly string _urlsEndpoint = "/api/urls";

    public static async Task<(string Username, string Password, AuthResponseDto body)>
        CreateRegisteredUserAsync(HttpClient client, string username, string password)
    {
        var payload = new RegisterRequestDto(username, password);

        var response = await client.PostAsJsonAsync(
            _registerEndpoint,
            payload
        );

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body.Should().NotBeNull();

        return (username, password, body);
    }

    public static async Task<ResponseUrlItemDto> CreateUrlItemAsync(HttpClient client, string url, string? token = null)
    {
        var createUrlItem = new CreateUrlItemDto(url);

        var postUrlItemRequest = new HttpRequestMessage(
            HttpMethod.Post,
            _urlsEndpoint
        )
        {
            Content = JsonContent.Create(createUrlItem)
        };
        postUrlItemRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(postUrlItemRequest);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseUrlItemDto = await response.Content.ReadFromJsonAsync<ResponseUrlItemDto>();
        responseUrlItemDto.Should().NotBeNull();

        return responseUrlItemDto;
    }
}
