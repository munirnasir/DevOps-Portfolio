using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Pos.Sales.Api.Security;

public interface IServiceTokenProvider
{
    string GetToken();
}

/// <summary>
/// Mints a short-lived service JWT (role Manager) so the Sales service can authenticate
/// its machine-to-machine calls to the Catalog. This reuses the shared symmetric signing
/// key — fine for this demo; a production system would use asymmetric keys or a dedicated
/// client-credentials flow so a single service can't impersonate users.
/// </summary>
public class ServiceTokenProvider : IServiceTokenProvider
{
    private readonly JwtOptions _options;

    public ServiceTokenProvider(IOptions<JwtOptions> options) => _options = options.Value;

    public string GetToken()
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "sales-service"),
            new Claim(ClaimTypes.Name, "Sales Service"),
            new Claim(ClaimTypes.Role, Roles.Manager),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

/// <summary>Attaches the service token to every outbound Catalog request.</summary>
public class ServiceAuthHandler : DelegatingHandler
{
    private readonly IServiceTokenProvider _tokens;

    public ServiceAuthHandler(IServiceTokenProvider tokens) => _tokens = tokens;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokens.GetToken());
        return base.SendAsync(request, cancellationToken);
    }
}
