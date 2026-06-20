namespace Pos.Identity.Api.Security;

/// <summary>
/// Symmetric JWT settings. The same Key/Issuer/Audience must be configured in the
/// Catalog and Sales services so they can validate tokens this service issues.
/// Bound from the "Jwt" configuration section.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "pos-identity";
    public string Audience { get; set; } = "pos-services";
    public int ExpiryMinutes { get; set; } = 480;
}
