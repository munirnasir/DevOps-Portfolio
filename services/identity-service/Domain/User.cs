namespace Pos.Identity.Api.Domain;

/// <summary>An operator who can sign in to the POS. Role drives authorization.</summary>
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>PBKDF2 hash produced by ASP.NET Core's <c>PasswordHasher</c>.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>One of <see cref="Roles"/>.</summary>
    public string Role { get; set; } = Roles.Cashier;

    public DateTime CreatedAtUtc { get; set; }
}

/// <summary>The roles recognized across the POS services.</summary>
public static class Roles
{
    public const string Manager = "Manager";
    public const string Cashier = "Cashier";

    public static readonly string[] All = [Manager, Cashier];

    public static bool IsValid(string role) => All.Contains(role);
}
