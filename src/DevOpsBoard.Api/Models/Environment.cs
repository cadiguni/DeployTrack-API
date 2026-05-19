namespace DevOpsBoard.Api.Models;

public sealed class Environment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    public ICollection<Deployment> Deployments { get; set; } = [];
    public ICollection<ApplicationHealthCheck> HealthChecks { get; set; } = [];
}
