using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dobby.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dobby.Tests.Integration;

public class AuthEndpointsTests : BaseIntegrationTest
{
    private readonly string _validPassword = "ValidPassword";
    private readonly string _myUrlsEndpoint = "/api/urls/my";
    private readonly string _registerEndpoint = "/api/auth/register";
    private readonly string _loginEndpoint = "/api/auth/login";

    public AuthEndpointsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task PostRegister_WithDuplicateUsername_Returns409Conflict()
    {
        // Arrange
        string randUsername = IntegrationTestHelpers.RandomUsername();
        var payload = new RegisterRequestDto(randUsername, _validPassword);

        // Act
        await _client.PostAsJsonAsync(_registerEndpoint, payload);
        var responseFailure = await _client.PostAsJsonAsync(_registerEndpoint, payload);

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
        var responseFailure = await _client.PostAsJsonAsync(_registerEndpoint, payload);

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
        await _client.PostAsJsonAsync(_registerEndpoint, payload);
        var responseFailure = await _client.PostAsJsonAsync(_registerEndpoint, payload);

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
        await _client.PostAsJsonAsync(_registerEndpoint, payload);
        var responseFailure = await _client.PostAsJsonAsync(_registerEndpoint, payload);

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
        await _client.PostAsJsonAsync(_registerEndpoint, payload);
        var responseFailure = await _client.PostAsJsonAsync(_registerEndpoint, payload);

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
        string randUsername = IntegrationTestHelpers.RandomUsername();
        var user = await IntegrationTestHelpers.CreateRegisteredUserAsync(_client, randUsername, _validPassword);
        var token = user.body.Token;

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            _myUrlsEndpoint
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
        string randUsername = IntegrationTestHelpers.RandomUsername();
        var user = await IntegrationTestHelpers.CreateRegisteredUserAsync(_client, randUsername, _validPassword);
        var payload = new LoginRequestDto(user.Username, user.Password);

        // Act
        var response = await _client.PostAsJsonAsync(_loginEndpoint, payload);

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
        string randUsername = IntegrationTestHelpers.RandomUsername();
        var user = await IntegrationTestHelpers.CreateRegisteredUserAsync(_client, randUsername, _validPassword);
        var loginPayload = new LoginRequestDto(user.Username, user.Password);
        var loginResponse = await _client.PostAsJsonAsync(
            _loginEndpoint,
            loginPayload
        );
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        loginBody.Should().NotBeNull();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            _myUrlsEndpoint
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
        string randUsername = IntegrationTestHelpers.RandomUsername();
        var user = await IntegrationTestHelpers.CreateRegisteredUserAsync(_client, randUsername, _validPassword);
        var payload = new LoginRequestDto("UnknownUsername", user.Password);

        // Act
        var response = await _client.PostAsJsonAsync(_loginEndpoint, payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostLogin_WithInvalidPassword_Returns401Unauthorized()
    {
        // Arrange
        string randUsername = IntegrationTestHelpers.RandomUsername();
        var user = await IntegrationTestHelpers.CreateRegisteredUserAsync(_client, randUsername, _validPassword);
        var payload = new LoginRequestDto(user.Username, "InvalidPassword");

        // Act
        var response = await _client.PostAsJsonAsync(_loginEndpoint, payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostLogin_WithEmptyUsername_Returns400BadRequest()
    {
        // Arrange
        var payload = new LoginRequestDto("", _validPassword);

        // Act
        var response = await _client.PostAsJsonAsync(_loginEndpoint, payload);

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
        string randUsername = IntegrationTestHelpers.RandomUsername();
        var payload = new LoginRequestDto(randUsername, "");

        // Act
        var response = await _client.PostAsJsonAsync(_loginEndpoint, payload);

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
