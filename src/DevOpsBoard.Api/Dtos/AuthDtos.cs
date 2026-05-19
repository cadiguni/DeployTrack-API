using DevOpsBoard.Api.Models;

namespace DevOpsBoard.Api.Dtos;

public sealed record RegisterUserRequest(string Name, string Email, string Password, UserRole Role);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(string Token, DateTimeOffset ExpiresAt, UserResponse User);

public sealed record UserResponse(Guid Id, string Name, string Email, UserRole Role);
