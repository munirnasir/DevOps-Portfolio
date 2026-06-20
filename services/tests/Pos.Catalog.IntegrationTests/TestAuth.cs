using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Pos.Catalog.IntegrationTests;

/// <summary>Mints JWTs that the API under test will accept, using a fixed test signing key.</summary>
public static class TestAuth
{
    public const string Key = "integration-test-signing-key-which-is-long-enough-1234567890";
    public const string Issuer = "pos-identity";
    public const string Audience = "pos-services";

    public static string TokenFor(string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var signing = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: signing);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
