using System.ComponentModel.DataAnnotations;

namespace Pos.Identity.Api.Dtos;

public record LoginRequest(
    [Required] string Username,
    [Required] string Password);

public record LoginResponse(
    string Token,
    DateTime ExpiresAtUtc,
    UserDto User);

public record UserDto(
    Guid Id,
    string Username,
    string DisplayName,
    string Role);

public record RegisterRequest(
    [Required, MaxLength(50)] string Username,
    [Required, MaxLength(100)] string DisplayName,
    [Required, MinLength(6)] string Password,
    [Required] string Role);
