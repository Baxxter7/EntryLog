using EntryLog.Business.JWT;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

namespace EntryLog.Tests.Business.JWT;

public class CustomBearerAuthenticationTests
{
    private readonly CustomBearerAuthentication _sut;

    public CustomBearerAuthenticationTests()
    {
        var options = Options.Create(new JwtConfiguration
        {
            Secret = "ThisIsATestSecretKeyThatIsLongEnoughForHmacSha256!!"
        });
        _sut = new CustomBearerAuthentication(options);
    }

    [Fact]
    public async Task GenerateTokenAsync_ReturnsValidJwtString()
    {
        var token = await _sut.GenerateTokenAsync("1001", "login", TimeSpan.FromMinutes(5));

        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateTokenAsync_TokenContainsExpectedClaims()
    {
        var token = await _sut.GenerateTokenAsync("1001", "faceid_reference", TimeSpan.FromMinutes(5));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "1001");
        jwt.Claims.Should().Contain(c => c.Type == "purpose" && c.Value == "faceid_reference");
    }

    [Fact]
    public async Task ValidateToken_ValidToken_ReturnsClaimsDictionary()
    {
        var token = await _sut.GenerateTokenAsync("1001", "login", TimeSpan.FromMinutes(5));

        var claims = _sut.ValidateToken(token);

        claims.Should().NotBeNull();
        claims!["sub"].Should().Be("1001");
        claims["purpose"].Should().Be("login");
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsNull()
    {
        var claims = _sut.ValidateToken("invalid.token.value");

        claims.Should().BeNull();
    }

    [Fact]
    public async Task ValidateToken_ExpiredToken_ReturnsNull()
    {
        var token = await _sut.GenerateTokenAsync("1001", "login", TimeSpan.FromMilliseconds(-1));

        var claims = _sut.ValidateToken(token);

        claims.Should().BeNull();
    }
}
