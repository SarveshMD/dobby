using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dobby.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dobby.Tests.Integration;

public class AuthEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly string _validUsername = "ValdiUsername";
    private readonly string _validPassword = "ValidPassword";

    public AuthEndpointsTests()
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

    [Fact]
    public async Task PostRegister_WithValidPayload_Returns201CreatedAndAuthResponseDto()
    {
        // Arrange
        var payload = new RegisterRequestDto(_validUsername, _validPassword);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        body.Should().NotBeNull();
        body.Id.Should().NotBe(Guid.Empty);
        body.Username.Should().Be(_validUsername);
        body.Message.Should().Be("Registration Successful");
        body.Token.Should().NotBeNullOrWhiteSpace();

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.ToString().Should().Be($"/users/{body.Id}");
    }

    [Fact]
    public async Task PostRegister_WithDuplicateUsername_Returns409Conflict()
    {
        // Arrange
        var payload = new RegisterRequestDto(_validUsername, _validPassword);

        // Act
        await _client.PostAsJsonAsync("/api/auth/register", payload);
        var responseFailure = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        responseFailure.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var message = await responseFailure.Content.ReadFromJsonAsync<string>();

        message.Should().Be("Username is taken");
    }

    [Fact]
    public async Task PostRegister_WithBadUsername_Returns400BadRequest()
    {
        // Arrange
        var payload = new RegisterRequestDto("invalid$$username", _validPassword);

        // Act
        var responseFailure = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        responseFailure.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        responseFailure.Content.Headers.ContentType!.MediaType
            .Should().Be("application/problem+json");

        var body = await responseFailure.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        body.Should().NotBeNull();
        body.Status.Should().Be(400);
        body.Errors.Should().NotBeEmpty();
    }


    [Fact]
    public async Task PostRegister_WithBadPassword_Returns400BadRequest()
    {
        // Arrange
        var payload = new RegisterRequestDto(_validPassword, "abc");

        // Act
        await _client.PostAsJsonAsync("/api/auth/register", payload);
        var responseFailure = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        responseFailure.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        responseFailure.Content.Headers.ContentType!.MediaType
            .Should().Be("application/problem+json");

        var body = await responseFailure.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        body.Should().NotBeNull();
        body.Status.Should().Be(400);
        body.Errors.Should().NotBeEmpty();
    }


    [Fact]
    public async Task PostRegister_WithEmptyUsername_Returns400BadRequest()
    {
        // Arrange
        var payload = new RegisterRequestDto("", _validPassword);

        // Act
        await _client.PostAsJsonAsync("/api/auth/register", payload);
        var responseFailure = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        responseFailure.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        responseFailure.Content.Headers.ContentType!.MediaType
            .Should().Be("application/problem+json");

        var body = await responseFailure.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        body.Should().NotBeNull();
        body.Status.Should().Be(400);
        body.Errors.Should().NotBeEmpty();
    }


    [Fact]
    public async Task PostRegister_WithEmptyPassword_Returns400BadRequest()
    {
        // Arrange
        var payload = new RegisterRequestDto(_validPassword, "");

        // Act
        await _client.PostAsJsonAsync("/api/auth/register", payload);
        var responseFailure = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        responseFailure.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        responseFailure.Content.Headers.ContentType!.MediaType
            .Should().Be("application/problem+json");

        var body = await responseFailure.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        body.Should().NotBeNull();
        body.Status.Should().Be(400);
        body.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PostRegister_WithValidPayload_ReturnsJwtTokenAcceptedByProtectedEndpoint()
    {
        // Arrange
        var user = await CreateRegisteredUserAsync();
        var token = user.body.Token;

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/urls/my-urls"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<IEnumerable<ResponseUrlItemDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task PostLogin_WithValidPayload_Returns200OkAndAuthResponseDto()
    {
        // Arrange
        var user = await CreateRegisteredUserAsync();
        var payload = new LoginRequestDto(user.Username, user.Password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        body.Should().NotBeNull();
        body.Id.Should().NotBe(Guid.Empty);
        body.Username.Should().Be(user.Username);
        body.Message.Should().Be("Login Successful");
        body.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task PostLogin_WithValidPayload_ReturnsJwtTokenAcceptedByProtectedEndpoint()
    {
        // Arrange
        var user = await CreateRegisteredUserAsync();
        var loginPayload = new LoginRequestDto(user.Username, user.Password);
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            loginPayload
        );
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        loginBody.Should().NotBeNull();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/urls/my-urls"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.Token);

        // Act
        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<IEnumerable<ResponseUrlItemDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task PostLogin_WithUnknownUsername_Returns401Unauthorized()
    {
        // Arrange
        var user = await CreateRegisteredUserAsync();
        var payload = new LoginRequestDto("UnknownUsername", user.Password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostLogin_WithInvalidPassword_Returns401Unauthorized()
    {
        // Arrange
        var user = await CreateRegisteredUserAsync();
        var payload = new LoginRequestDto(user.Username, "InvalidPassword");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostLogin_WithEmptyUsername_Returns400BadRequest()
    {
        // Arrange
        var payload = new LoginRequestDto("", _validPassword);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        response.Content.Headers.ContentType!.MediaType
            .Should().Be("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        body.Should().NotBeNull();
        body.Status.Should().Be(400);
        body.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PostLogin_WithEmptyPassword_Returns400BadRequest()
    {
        // Arrange
        var payload = new LoginRequestDto(_validUsername, "");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        response.Content.Headers.ContentType!.MediaType
            .Should().Be("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        body.Should().NotBeNull();
        body.Status.Should().Be(400);
        body.Errors.Should().NotBeEmpty();
    }
}
