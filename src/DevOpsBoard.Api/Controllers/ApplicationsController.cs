using DevOpsBoard.Api.Data;
using DevOpsBoard.Api.Dtos;
using DevOpsBoard.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/applications")]
public sealed class ApplicationsController(DevOpsBoardDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ApplicationResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var applications = await dbContext.Applications
            .AsNoTracking()
            .OrderBy(application => application.Name)
            .Select(application => ToResponse(application))
            .ToListAsync(cancellationToken);

        return Ok(applications);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApplicationResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var application = await dbContext.Applications
            .AsNoTracking()
            .SingleOrDefaultAsync(application => application.Id == id, cancellationToken);

        return application is null ? NotFound() : Ok(ToResponse(application));
    }

    [Authorize(Roles = "Admin,DevOps")]
    [HttpPost]
    public async Task<ActionResult<ApplicationResponse>> Create(ApplicationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Application name is required.");
        }

        var normalizedName = request.Name.Trim().ToLowerInvariant();
        var exists = await dbContext.Applications.AnyAsync(application => application.Name == normalizedName, cancellationToken);

        if (exists)
        {
            return Conflict("Application already exists.");
        }

        var application = new Application
        {
            Name = normalizedName,
            Description = request.Description?.Trim(),
            RepositoryUrl = request.RepositoryUrl?.Trim()
        };

        dbContext.Applications.Add(application);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = application.Id }, ToResponse(application));
    }

    [Authorize(Roles = "Admin,DevOps")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApplicationResponse>> Update(Guid id, ApplicationRequest request, CancellationToken cancellationToken)
    {
        var application = await dbContext.Applications.SingleOrDefaultAsync(application => application.Id == id, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Application name is required.");
        }

        var normalizedName = request.Name.Trim().ToLowerInvariant();
        var nameInUse = await dbContext.Applications
            .AnyAsync(other => other.Id != id && other.Name == normalizedName, cancellationToken);

        if (nameInUse)
        {
            return Conflict("Application name is already in use.");
        }

        application.Name = normalizedName;
        application.Description = request.Description?.Trim();
        application.RepositoryUrl = request.RepositoryUrl?.Trim();
        application.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(application));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var application = await dbContext.Applications.SingleOrDefaultAsync(application => application.Id == id, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        var hasHistory = await dbContext.Deployments.AnyAsync(deployment => deployment.ApplicationId == id, cancellationToken) ||
            await dbContext.HealthChecks.AnyAsync(healthCheck => healthCheck.ApplicationId == id, cancellationToken);

        if (hasHistory)
        {
            return Conflict("Application has deployments or health checks and cannot be removed.");
        }

        dbContext.Applications.Remove(application);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static ApplicationResponse ToResponse(Application application) =>
        new(application.Id, application.Name, application.Description, application.RepositoryUrl, application.CreatedAt, application.UpdatedAt);
}
