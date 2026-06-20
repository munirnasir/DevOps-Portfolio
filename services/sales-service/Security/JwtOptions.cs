namespace Pos.Sales.Api.Security;

/// <summary>Recognized roles (must match the Identity service).</summary>
public static class Roles
{
    public const string Manager = "Manager";
    public const string Cashier = "Cashier";
}

/// <summary>
/// Shared symmetric JWT settings, bound from the "Jwt" section. Used both to validate
/// incoming user tokens and to mint the outbound service token for Catalog calls.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "pos-identity";
    public string Audience { get; set; } = "pos-services";
}
