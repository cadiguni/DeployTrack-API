using DevOpsBoard.Api.Data;
using DevOpsBoard.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/environments")]
public sealed class EnvironmentsController(DevOpsBoardDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EnvironmentResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var environments = await dbContext.Environments
            .AsNoTracking()
            .OrderBy(environment => environment.Name)
            .Select(environment => new EnvironmentResponse(environment.Id, environment.Name))
            .ToListAsync(cancellationToken);

        return Ok(environments);
    }
}
