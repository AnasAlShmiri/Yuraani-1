namespace Yuraani.Application.Features.Identity.Auth.DTOs;
public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, Guid UserAccountId,
    string FullName, string PhoneNumber, string Email, string? RefreshToken = null, DateTimeOffset? RefreshTokenExpiresAt = null);