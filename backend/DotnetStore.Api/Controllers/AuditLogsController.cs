using DotnetStore.Api.Authorization;
using DotnetStore.Api.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajDb;

namespace DotnetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AuthRoles.Admin)]
public class AuditLogsController : ControllerBase
{
    private readonly DataContext _db;

    public AuditLogsController(DataContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLogListItemDto>>> GetRecent(
        [FromQuery] int take = 300,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var list = await (
                from a in _db.AuditLogs.AsNoTracking()
                join u in _db.Users.AsNoTracking() on a.UserId equals u.Id
                orderby a.CreatedAtUtc descending
                select new AuditLogListItemDto(
                    a.Id,
                    u.Email,
                    u.UserName,
                    a.Action,
                    a.Details,
                    a.CreatedAtUtc))
            .Take(take)
            .ToListAsync(ct);
        return Ok(list);
    }
}
