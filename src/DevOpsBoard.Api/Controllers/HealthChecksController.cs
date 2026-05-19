using DevOpsBoard.Api.Data;
using DevOpsBoard.Api.Dtos;
using DevOpsBoard.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Environment = DevOpsBoard.Api.Models.Environment;

namespace DevOpsBoard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/health-checks")]
public sealed class HealthChecksController(DevOpsBoardDbContext dbContext) : ControllerBase
{
    [Authorize(Roles = "Admin,DevOps")]
    [HttpPost]
    public async Task<ActionResult<HealthCheckResponse>> Create(CreateHealthCheckRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ApplicationName) || string.IsNullOrWhiteSpace(request.Environment))
        {
            return BadRequest("ApplicationName and environment are required.");
        }

        var application = await FindApplication(request.ApplicationName, cancellationToken);
        var environment = await FindEnvironment(request.Environment, cancellationToken);

        if (application is null)
        {
            return NotFound("Application was not found.");
        }

        if (environment is null)
        {
            return NotFound("Environment was not found.");
        }

        var healthCheck = new ApplicationHealthCheck
        {
            ApplicationId = application.Id,
            EnvironmentId = environment.Id,
            Status = request.Status,
            Details = request.Details?.Trim(),
            CheckedBy = request.CheckedBy?.Trim(),
            CheckedAt = request.CheckedAt ?? DateTimeOffset.UtcNow
        };

        dbContext.HealthChecks.Add(healthCheck);
        await dbContext.SaveChangesAsync(cancellationToken);

        healthCheck.Application = application;
        healthCheck.Environment = environment;

        return CreatedAtAction(nameof(GetCurrent), new { applicationName = application.Name, environment = environment.Name }, ToResponse(healthCheck));
    }

    [HttpGet("current")]
    public async Task<ActionResult<HealthCheckResponse>> GetCurrent(
        [FromQuery] string applicationName,
        [FromQuery] string environment,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(applicationName) || string.IsNullOrWhiteSpace(environment))
        {
            return BadRequest("ApplicationName and environment are required.");
        }

        var normalizedApplicationName = applicationName.Trim().ToLowerInvariant();
        var normalizedEnvironment = environment.Trim().ToLowerInvariant();

        var healthCheck = await HealthCheckQuery()
            .Where(check => check.Application.Name == normalizedApplicationName && check.Environment.Name == normalizedEnvironment)
            .OrderByDescending(check => check.CheckedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return healthCheck is null ? NotFound() : Ok(ToResponse(healthCheck));
    }

    private IQueryable<ApplicationHealthCheck> HealthCheckQuery() =>
        dbContext.HealthChecks.AsNoTracking().Include(check => check.Application).Include(check => check.Environment);

    private async Task<Application?> FindApplication(string name, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        return await dbContext.Applications.SingleOrDefaultAsync(application => application.Name == normalizedName, cancellationToken);
    }

    private async Task<Environment?> FindEnvironment(string name, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        return await dbContext.Environments.SingleOrDefaultAsync(environment => environment.Name == normalizedName, cancellationToken);
    }

    private static HealthCheckResponse ToResponse(ApplicationHealthCheck healthCheck) =>
        new(
            healthCheck.Id,
            healthCheck.Application.Name,
            healthCheck.Environment.Name,
            healthCheck.Status,
            healthCheck.Details,
            healthCheck.CheckedBy,
            healthCheck.CheckedAt);
}
