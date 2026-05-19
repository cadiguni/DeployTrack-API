namespace DevOpsBoard.Api.Models;

public sealed class ApplicationHealthCheck
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApplicationId { get; set; }
    public Guid EnvironmentId { get; set; }
    public HealthStatus Status { get; set; }
    public string? Details { get; set; }
    public string? CheckedBy { get; set; }
    public DateTimeOffset CheckedAt { get; set; } = DateTimeOffset.UtcNow;

    public Application Application { get; set; } = default!;
    public Environment Environment { get; set; } = default!;
}
