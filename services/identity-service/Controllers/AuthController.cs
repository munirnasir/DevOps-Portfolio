using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Identity.Api.Data;
using Pos.Identity.Api.Domain;
using Pos.Identity.Api.Dtos;
using Pos.Identity.Api.Security;

namespace Pos.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IdentityDbContext _db;
    private readonly IPasswordHasher<User> _hasher;
    private readonly ITokenService _tokens;

    public AuthController(IdentityDbContext db, IPasswordHasher<User> hasher, ITokenService tokens)
    {
        _db = db;
        _hasher = hasher;
        _tokens = tokens;
    }

    /// <summary>Exchange username + password for a JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var (token, expires) = _tokens.CreateToken(user);
        return Ok(new LoginResponse(token, expires, ToDto(user)));
    }

    /// <summary>Create a new operator account. Managers only.</summary>
    [HttpPost("register")]
    [Authorize(Roles = Roles.Manager)]
    public async Task<ActionResult<UserDto>> Register(RegisterRequest request)
    {
        if (!Roles.IsValid(request.Role))
        {
            return Problem($"Role must be one of: {string.Join(", ", Roles.All)}.", statusCode: StatusCodes.Status400BadRequest);
        }

        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
        {
            return Conflict(new { message = $"Username '{request.Username}' is taken." });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            DisplayName = request.DisplayName,
            Role = request.Role,
            CreatedAtUtc = DateTime.UtcNow
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Me), ToDto(user));
    }

    /// <summary>Return the currently authenticated user.</summary>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<UserDto> Me()
    {
        var id = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                 ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName) ?? string.Empty;
        var displayName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        return Ok(new UserDto(Guid.TryParse(id, out var guid) ? guid : Guid.Empty, username, displayName, role));
    }

    private static UserDto ToDto(User u) => new(u.Id, u.Username, u.DisplayName, u.Role);
}
