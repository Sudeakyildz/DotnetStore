using DotnetStore.Api.Authorization;
using DotnetStore.Api.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajDb;
using StajDb.Models;

namespace DotnetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AuthRoles.Admin)]
public class UsersController : ControllerBase
{
    private readonly DataContext _db;

    public UsersController(DataContext db)
    {
        _db = db;
    }

    [HttpGet("musteri")]
    public async Task<ActionResult<IEnumerable<UserListItemDto>>> GetMusteriOnlyForOrders(CancellationToken ct)
    {
        var list = await _db.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Musteri)
            .OrderBy(u => u.Email)
            .Select(u => new UserListItemDto(
                u.Id,
                u.Email,
                u.UserName,
                u.Role.ToString(),
                u.FirstLoginAt,
                u.LastLoginAt,
                u.CreatedAt,
                u.UpdatedAt))
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserListItemDto>>> GetAll(CancellationToken ct)
    {
        var list = await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .Select(u => new UserListItemDto(
                u.Id,
                u.Email,
                u.UserName,
                u.Role.ToString(),
                u.FirstLoginAt,
                u.LastLoginAt,
                u.CreatedAt,
                u.UpdatedAt))
            .ToListAsync(ct);
        return Ok(list);
    }
}
