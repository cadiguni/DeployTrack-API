using DevOpsBoard.Api.Data;
using DevOpsBoard.Api.Dtos;
using DevOpsBoard.Api.Models;
using DevOpsBoard.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    DevOpsBoardDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Name, email and password are required.");
        }

        if (request.Password.Length < 8)
        {
            return BadRequest("Password must contain at least 8 characters.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailAlreadyExists = await dbContext.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (emailAlreadyExists)
        {
            return Conflict("Email is already registered.");
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = request.Role
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        var (token, expiresAt) = jwtTokenService.CreateToken(user);

        return CreatedAtAction(nameof(Register), new AuthResponse(token, expiresAt, ToResponse(user)));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.SingleOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid email or password.");
        }

        var (token, expiresAt) = jwtTokenService.CreateToken(user);

        return Ok(new AuthResponse(token, expiresAt, ToResponse(user)));
    }

    private static UserResponse ToResponse(User user) => new(user.Id, user.Name, user.Email, user.Role);
}
