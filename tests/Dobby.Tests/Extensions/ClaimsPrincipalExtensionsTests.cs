using System.Security.Claims;
using Dobby.Extensions;
using FluentAssertions;

namespace Dobby.Tests.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims) =>
        new(new ClaimsIdentity(claims));

    [Fact]
    public void GetUserId_WithNameIdentifierClaim_ShouldReturnClaimGuid()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var user = CreatePrincipal(new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString()));

        // Act
        var result = user.GetUserId();

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetUserId_WithSubClaim_ShouldReturnClaimGuid()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var user = CreatePrincipal(new Claim("sub", expectedUserId.ToString()));

        // Act
        var result = user.GetUserId();

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetUserId_WithBothClaims_ShouldPreferNameIdentifierClaim()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var user = CreatePrincipal(
            new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString()),
            new Claim("sub", Guid.NewGuid().ToString()));

        // Act
        var result = user.GetUserId();

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetUserId_WhenUserIdClaimIsMissing_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var act = () => user.GetUserId();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("User ID claim is missing from the authenticated principal");
    }

    [Theory]
    [InlineData(ClaimTypes.NameIdentifier)]
    [InlineData("sub")]
    public void GetUserId_WhenClaimValueIsNotGuid_ShouldThrowFormatException(string claimType)
    {
        // Arrange
        var user = CreatePrincipal(new Claim(claimType, "not-a-guid"));

        // Act
        var act = () => user.GetUserId();

        // Assert
        act.Should().Throw<FormatException>();
    }
}
