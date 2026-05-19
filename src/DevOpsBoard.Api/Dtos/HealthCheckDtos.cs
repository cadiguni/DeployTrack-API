using DevOpsBoard.Api.Models;

namespace DevOpsBoard.Api.Dtos;

public sealed record CreateHealthCheckRequest(
    string ApplicationName,
    string Environment,
    HealthStatus Status,
    string? Details,
    string? CheckedBy,
    DateTimeOffset? CheckedAt);

public sealed record HealthCheckResponse(
    Guid Id,
    string ApplicationName,
    string Environment,
    HealthStatus Status,
    string? Details,
    string? CheckedBy,
    DateTimeOffset CheckedAt);
