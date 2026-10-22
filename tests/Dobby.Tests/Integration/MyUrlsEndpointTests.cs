using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dobby.DTOs;
using FluentAssertions;

namespace Dobby.Tests.Integration;

public class MyUrlsEndpointTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly string _validUsername = "ValdiUsername";
    private readonly string _validPassword = "ValidPassword";

    public MyUrlsEndpointTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<(string Username, string Password, AuthResponseDto body)>
        CreateRegisteredUserAsync(string username, string password)
    {
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

    private async Task<ResponseUrlItemDto> CreateUrlItemAsync(string url, string token)
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
    public async Task GetMyUrls_AuthenticatedUserWithNoUrls_Returns200OkWithEmptyBody()
    {
        // Arrange
        var (_, _, body) = await CreateRegisteredUserAsync(_validUsername, _validPassword);
        var token = body.Token;

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/urls/my-urls"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);
        var getResponseBody = await response.Content.ReadFromJsonAsync<IEnumerable<ResponseUrlItemDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        getResponseBody.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMyUrls_AuthenticatedUserWithSomeUrls_Returns200OkWithCreatedItemsInBodyOrderedByCreationTime()
    {
        // Arrange
        var (_, _, body) = await CreateRegisteredUserAsync(_validUsername, _validPassword);
        var token = body.Token;

        string[] urls =
        [
            "https://www.goodreads.com/book/show/25864867-paper-towns",
            "https://letterboxd.com/film/paper-towns/"
        ];

        var responseUrlItemDtos = new List<ResponseUrlItemDto>();

        foreach (string url in urls)
        {
            responseUrlItemDtos.Add(await CreateUrlItemAsync(url, token));
        }

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/urls/my-urls"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);
        var getResponseBody = await response.Content.ReadFromJsonAsync<IEnumerable<ResponseUrlItemDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        getResponseBody.Should().NotBeNullOrEmpty();
        getResponseBody.Should()
            .BeEquivalentTo(responseUrlItemDtos, options => options.WithStrictOrdering());

    }

    [Fact]
    public async Task GetMyUrls_UnauthenticatedUser_Gets401Unauthorized()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("/api/urls/my-urls");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyUrls_InvalidTokenUser_Gets401Unauthorized()
    {
        // Arrange
        string dummyToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/urls/my-urls"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", dummyToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyUrls_TwoValidUsers_CannotGetEachOthersItems()
    {
        // Arrange
        var user1 = await CreateRegisteredUserAsync("user1", _validPassword);
        var user2 = await CreateRegisteredUserAsync("user2", _validPassword);

        var responseUrlItemOne = await CreateUrlItemAsync("https://letterboxd.com/film/spider-man-no-way-home/", user1.body.Token);
        var responseUrlItemTwo = await CreateUrlItemAsync("https://letterboxd.com/film/obsession-2025/", user2.body.Token);

        var getRequestOne = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/urls/my-urls"
        );
        getRequestOne.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user1.body.Token);

        var getRequestTwo = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/urls/my-urls"
        );
        getRequestTwo.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user2.body.Token);

        // Act
        var responseOne = await _client.SendAsync(getRequestOne);
        var responseTwo = await _client.SendAsync(getRequestTwo);

        var responseOneBody = await responseOne.Content.ReadFromJsonAsync<IEnumerable<ResponseUrlItemDto>>();
        var responseTwoBody = await responseTwo.Content.ReadFromJsonAsync<IEnumerable<ResponseUrlItemDto>>();

        // Assert
        responseOne.StatusCode.Should().Be(HttpStatusCode.OK);
        responseOneBody.Should().HaveCount(1);
        responseOneBody.First().Should().BeEquivalentTo(responseUrlItemOne);

        responseTwo.StatusCode.Should().Be(HttpStatusCode.OK);
        responseTwoBody.Should().HaveCount(1);
        responseTwoBody.First().Should().BeEquivalentTo(responseUrlItemTwo);
    }
}
