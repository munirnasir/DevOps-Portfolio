using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Pos.Catalog.Api.Security;

/// <summary>Recognized roles (must match the Identity service).</summary>
public static class Roles
{
    public const string Manager = "Manager";
    public const string Cashier = "Cashier";
}

/// <summary>
/// Configures JWT bearer validation against the tokens minted by the Identity service.
/// Key/Issuer/Audience come from the shared "Jwt" config section and must match Identity.
/// </summary>
public static class JwtAuthentication
{
    public static IServiceCollection AddPosJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var key = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var issuer = config["Jwt:Issuer"] ?? "pos-identity";
        var audience = config["Jwt:Audience"] ?? "pos-services";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });
        services.AddAuthorization();
        return services;
    }
}
