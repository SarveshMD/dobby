using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Dobby.Models;
using Dobby.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Dobby.Tests.Services;

public class TokenServiceTests
{
    private const string JwtKey = "A_VERY_LONG_SECRET_KEY_THAT_IS_AT_LEAST_32_BYTES_LONG_FOR_SECURITY";
    private const string Issuer = "DobbyBackend";
    private const string Audience = "DobbyClient";
    private static TokenService CreateSut(int? expiryMinutes = null, bool includeKey = true)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = Issuer,
            ["Jwt:Audience"] = Audience
        };

        if (includeKey)
        {
            settings["Jwt:Key"] = JwtKey;
        }

        if (expiryMinutes.HasValue)
        {
            settings["Jwt:ExpiryMinutes"] = expiryMinutes.Value.ToString();
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        return new TokenService(configuration);
    }

    private static JwtSecurityToken ReadToken(string serializedToken) =>
        new JwtSecurityTokenHandler().ReadJwtToken(serializedToken);


    [Fact]
    public void GenerateToken_ShouldReturnWellFormedJwtWithThreeSegments()
    {
        // Arrange
        var sut = CreateSut();
        var user = new User("test-user", "hashed-password");

        // Act
        var token = sut.GenerateToken(user);
        var segments = token.Split('.');

        // Assert
        segments.Should().HaveCount(3);
        segments.Should().OnlyContain(segment => !string.IsNullOrWhiteSpace(segment));
    }

    [Fact]
    public void GenerateToken_ShouldContainUserClaimsAndConfiguredMetadata()
    {
        // Arrange
        var user = new User("test-user", "hashed-password");
        var sut = CreateSut();

        // Act
        var token = ReadToken(sut.GenerateToken(user));

        // Assert
        token.Subject.Should().Be(user.Id.ToString());
        token.Claims.Should().ContainSingle(claim =>
            claim.Type == JwtRegisteredClaimNames.PreferredUsername && claim.Value == user.Username);
        token.Claims.Should().ContainSingle(claim => claim.Type == JwtRegisteredClaimNames.Jti);
        token.Issuer.Should().Be(Issuer);
        token.Audiences.Should().ContainSingle().Which.Should().Be(Audience);
    }

    [Fact]
    public void GenerateToken_ShouldBeSignedWithConfiguredKey()
    {
        // Arrange
        var user = new User("test-user", "hashed-password");
        var sut = CreateSut();
        var tokenHandler = new JwtSecurityTokenHandler();

        // Act
        var act = () => tokenHandler.ValidateToken(
            sut.GenerateToken(user),
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateLifetime = false
            },
            out _);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateToken_ShouldUseConfiguredExpiry()
    {
        // Arrange
        const int expiryMinutes = 15;
        var sut = CreateSut(expiryMinutes);
        var beforeGeneration = DateTime.UtcNow.AddMinutes(expiryMinutes);

        // Act
        var token = ReadToken(sut.GenerateToken(new User("test-user", "hashed-password")));
        var afterGeneration = DateTime.UtcNow.AddMinutes(expiryMinutes);

        // Assert
        token.ValidTo.Should().BeOnOrAfter(beforeGeneration.AddSeconds(-1));
        token.ValidTo.Should().BeOnOrBefore(afterGeneration.AddSeconds(1));
    }

    [Fact]
    public void GenerateToken_WhenKeyIsMissing_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var sut = CreateSut(includeKey: false);
        var act = () => sut.GenerateToken(new User("test-user", "hashed-password"));

        // Act and Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("JWT Key is not configured");
    }

}
