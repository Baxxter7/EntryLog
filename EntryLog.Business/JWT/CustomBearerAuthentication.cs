using EntryLog.Business.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EntryLog.Business.JWT;

internal class CustomBearerAuthentication : IJwtService
{
    private readonly JwtConfiguration _jwtConfiguration;

    public CustomBearerAuthentication(IOptions<JwtConfiguration> options)
    {
        _jwtConfiguration = options.Value;
    }

    public Task<string> GenerateTokenAsync(string userId, string purpose, TimeSpan expiresIn)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new("purpose", purpose),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.Add(expiresIn),
            signingCredentials: credentials);

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public IDictionary<string, string>? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                LifetimeValidator = JwtValidator.CustomLifeTimeValidator,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Secret)),
                ValidateIssuerSigningKey = true
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            var claimsDictionary = new Dictionary<string, string>();

            foreach( var claim in principal.Claims)
                claimsDictionary[claim.Type] = claim.Value;

            return claimsDictionary;
        }
        catch
        {
            return null;
        }
        
    }
}
