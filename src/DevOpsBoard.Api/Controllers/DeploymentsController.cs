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
[Route("api/deployments")]
public sealed class DeploymentsController(DevOpsBoardDbContext dbContext) : ControllerBase
{
    [Authorize(Roles = "Admin,DevOps")]
    [HttpPost]
    public async Task<ActionResult<DeploymentResponse>> Create(CreateDeploymentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ApplicationName) ||
            string.IsNullOrWhiteSpace(request.Environment) ||
            string.IsNullOrWhiteSpace(request.Version))
        {
            return BadRequest("ApplicationName, environment and version are required.");
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

        var deployment = new Deployment
        {
            ApplicationId = application.Id,
            EnvironmentId = environment.Id,
            Version = request.Version.Trim(),
            Status = request.Status,
            DeployedBy = request.DeployedBy.Trim(),
            CommitSha = request.CommitSha.Trim(),
            PipelineUrl = request.PipelineUrl?.Trim(),
            StartedAt = request.StartedAt,
            FinishedAt = request.FinishedAt
        };

        dbContext.Deployments.Add(deployment);
        await dbContext.SaveChangesAsync(cancellationToken);

        deployment.Application = application;
        deployment.Environment = environment;

        return CreatedAtAction(nameof(GetById), new { id = deployment.Id }, ToResponse(deployment));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeploymentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var deployment = await DeploymentQuery()
            .SingleOrDefaultAsync(deployment => deployment.Id == id, cancellationToken);

        return deployment is null ? NotFound() : Ok(ToResponse(deployment));
    }

    [HttpGet("latest")]
    public async Task<ActionResult<DeploymentResponse>> GetLatest(
        [FromQuery] string applicationName,
        [FromQuery] string? environment,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(applicationName))
        {
            return BadRequest("ApplicationName is required.");
        }

        var normalizedApplicationName = applicationName.Trim().ToLowerInvariant();
        var normalizedEnvironment = environment?.Trim().ToLowerInvariant();

        var query = DeploymentQuery()
            .Where(deployment => deployment.Application.Name == normalizedApplicationName);

        if (!string.IsNullOrWhiteSpace(normalizedEnvironment))
        {
            query = query.Where(deployment => deployment.Environment.Name == normalizedEnvironment);
        }

        var deployment = await query
            .OrderByDescending(deployment => deployment.FinishedAt ?? deployment.StartedAt)
            .ThenByDescending(deployment => deployment.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return deployment is null ? NotFound() : Ok(ToResponse(deployment));
    }

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<DeploymentResponse>>> GetHistory(
        [FromQuery] string? applicationName,
        [FromQuery] string? environment,
        CancellationToken cancellationToken)
    {
        var query = DeploymentQuery();

        if (!string.IsNullOrWhiteSpace(applicationName))
        {
            var normalizedApplicationName = applicationName.Trim().ToLowerInvariant();
            query = query.Where(deployment => deployment.Application.Name == normalizedApplicationName);
        }

        if (!string.IsNullOrWhiteSpace(environment))
        {
            var normalizedEnvironment = environment.Trim().ToLowerInvariant();
            query = query.Where(deployment => deployment.Environment.Name == normalizedEnvironment);
        }

        var deployments = await query
            .OrderByDescending(deployment => deployment.StartedAt)
            .Select(deployment => ToResponse(deployment))
            .ToListAsync(cancellationToken);

        return Ok(deployments);
    }

    private IQueryable<Deployment> DeploymentQuery() =>
        dbContext.Deployments.AsNoTracking().Include(deployment => deployment.Application).Include(deployment => deployment.Environment);

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

    private static DeploymentResponse ToResponse(Deployment deployment) =>
        new(
            deployment.Id,
            deployment.Application.Name,
            deployment.Environment.Name,
            deployment.Version,
            deployment.Status,
            deployment.DeployedBy,
            deployment.CommitSha,
            deployment.PipelineUrl,
            deployment.StartedAt,
            deployment.FinishedAt,
            deployment.CreatedAt);
}
