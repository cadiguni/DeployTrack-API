using DevOpsBoard.Api.Models;

namespace DevOpsBoard.Api.Dtos;

public sealed record CreateDeploymentRequest(
    string ApplicationName,
    string Environment,
    string Version,
    DeploymentStatus Status,
    string DeployedBy,
    string CommitSha,
    string? PipelineUrl,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt);

public sealed record UpdateDeploymentRequest(
    string ApplicationName,
    string Environment,
    string Version,
    DeploymentStatus Status,
    string DeployedBy,
    string CommitSha,
    string? PipelineUrl,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt);

public sealed record DeploymentResponse(
    Guid Id,
    string ApplicationName,
    string Environment,
    string Version,
    DeploymentStatus Status,
    string DeployedBy,
    string CommitSha,
    string? PipelineUrl,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    DateTimeOffset CreatedAt);
