using Dobby.Services;
using FluentAssertions;
using Xunit;

namespace Dobby.Tests;

public class UrlShortenerTests
{
    private const string ValidKey = "1234567890";
    private const string AltKey = "0987654321";

    private UrlShortener CreateSut(string key = ValidKey) => new UrlShortener(key);

    [Fact]
    public void Encode_ShouldAlwaysReturnSixCharacters()
    {
        // Arrange
        var sut = CreateSut();
        int input = 42;

        // Act
        string code = sut.Encode(input);

        // Assert
        code.Should().HaveLength(6);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(1_000_000)]
    [InlineData(int.MaxValue)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void EncodeAndDecode_ShouldPreserveOriginalValue(int originalValue)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        string encoded = sut.Encode(originalValue);
        int decoded = sut.Decode(encoded);

        // Assert
        decoded.Should().Be(originalValue);

    }

    [Fact]
    public void Encode_SameInputWithSameKey_ShouldBeDeterministic()
    {
        // Arrange
        var sut1 = CreateSut();
        var sut2 = CreateSut();
        var input = 1984;

        // Act
        string res1 = sut1.Encode(input);
        string res2 = sut2.Encode(input);

        // Assert
        res1.Should().Be(res2);
    }

    [Fact]
    public void Encode_DifferentKeys_ShouldProduceDifferentCodes()
    {
        // Arrange
        var sut1 = CreateSut(ValidKey);
        var sut2 = CreateSut(AltKey);
        int input = 1856;

        // Act
        string res1 = sut1.Encode(input);
        string res2 = sut2.Encode(input);

        // Assert
        res1.Should().NotBe(res2);
    }

    [Fact]
    public void Encode_ConsecutiveNumbers_ShouldNotCollide()
    {
        // Arrange
        var sut = CreateSut();
        const int count = 1000;

        // Act
        var generatedCodes = Enumerable.Range(1, count)
            .Select(sut.Encode)
            .ToHashSet();

        // Assert
        generatedCodes.Should().HaveCount(count);
    }

    [Fact]
    public void Constructor_WhenKeyIsNotTenBytes_ThrowsArgumentException()
    {
        // Arrange
        var act = () => new UrlShortener("a very bad key");

        // Act and Assert
        act.Should().Throw<ArgumentException>();

    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Decode_NullOrWhitespace_ThrowsArgumentException(string? invalidCode)
    {
        // Arrange
        var sut = CreateSut();
        var act = () => sut.Decode(invalidCode!);

        // Act and Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("a")]
    [InlineData("abc")]
    [InlineData("abcde")]
    [InlineData("abcdefg")]
    public void Decode_LengthNotSix_ThrowsArgumentException(string invalidCode)
    {
        // Arrange
        var sut = CreateSut();
        var act = () => sut.Decode(invalidCode);

        // Act and Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("abc-ef")]
    [InlineData("abc_ef")]
    [InlineData("abc ef")]
    [InlineData("abc!ef")]
    [InlineData("abc💀ef")]
    public void Decode_InvalidCharacters_ThrowsArgumentException(string invalidCode)
    {
        // Arrange
        var sut = CreateSut();
        var act = () => sut.Decode(invalidCode);

        // Act and Assert
        act.Should().Throw<ArgumentException>();
    }

}
