namespace DevOpsBoard.Api.Dtos;

public sealed record ApplicationRequest(string Name, string? Description, string? RepositoryUrl);

public sealed record ApplicationResponse(
    Guid Id,
    string Name,
    string? Description,
    string? RepositoryUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
