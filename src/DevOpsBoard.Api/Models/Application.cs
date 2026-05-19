namespace DevOpsBoard.Api.Models;

public sealed class Application
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RepositoryUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<Deployment> Deployments { get; set; } = [];
    public ICollection<ApplicationHealthCheck> HealthChecks { get; set; } = [];
}
