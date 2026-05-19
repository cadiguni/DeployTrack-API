namespace DevOpsBoard.Api.Models;

public sealed class Deployment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApplicationId { get; set; }
    public Guid EnvironmentId { get; set; }
    public string Version { get; set; } = string.Empty;
    public DeploymentStatus Status { get; set; }
    public string DeployedBy { get; set; } = string.Empty;
    public string CommitSha { get; set; } = string.Empty;
    public string? PipelineUrl { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Application Application { get; set; } = default!;
    public Environment Environment { get; set; } = default!;
}
