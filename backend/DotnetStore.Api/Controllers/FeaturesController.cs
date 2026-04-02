using DotnetStore.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajDb;
using StajDb.Models;

namespace DotnetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeaturesController : ControllerBase
{
    private readonly DataContext _db;

    public FeaturesController(DataContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FeatureResponseDto>>> GetAll(CancellationToken ct)
    {
        var list = await _db.Features
            .AsNoTracking()
            .Where(f => !f.IsDeleted)
            .OrderBy(f => f.Name)
            .Select(f => new FeatureResponseDto(f.Id, f.Name, f.DataType, f.CreatedAt, f.UpdatedAt))
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FeatureResponseDto>> GetById(int id, CancellationToken ct)
    {
        var f = await _db.Features
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new FeatureResponseDto(x.Id, x.Name, x.DataType, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync(ct);
        if (f is null) return NotFound();
        return Ok(f);
    }

    [HttpPost]
    public async Task<ActionResult<FeatureResponseDto>> Create([FromBody] FeatureCreateDto dto, CancellationToken ct)
    {
        var name = dto.Name.Trim();
        var exists = await _db.Features.AnyAsync(x => !x.IsDeleted && x.Name == name, ct);
        if (exists)
            return Conflict(new { message = "Bu isimde bir özellik zaten var." });

        var now = DateTime.UtcNow;
        var entity = new Feature
        {
            Name = name,
            DataType = string.IsNullOrWhiteSpace(dto.DataType) ? null : dto.DataType.Trim(),
            IsDeleted = false,
            CreatedBy = "api",
            CreatedAt = now,
            UpdatedBy = "api",
            UpdatedAt = now
        };
        _db.Features.Add(entity);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id },
            new FeatureResponseDto(entity.Id, entity.Name, entity.DataType, entity.CreatedAt, entity.UpdatedAt));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] FeatureUpdateDto dto, CancellationToken ct)
    {
        var entity = await _db.Features.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null) return NotFound();

        var name = dto.Name.Trim();
        var taken = await _db.Features.AnyAsync(x => !x.IsDeleted && x.Id != id && x.Name == name, ct);
        if (taken)
            return Conflict(new { message = "Bu isimde bir özellik zaten var." });

        var now = DateTime.UtcNow;
        entity.Name = name;
        entity.DataType = string.IsNullOrWhiteSpace(dto.DataType) ? null : dto.DataType.Trim();
        entity.UpdatedBy = "api";
        entity.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await _db.Features.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null) return NotFound();

        var now = DateTime.UtcNow;
        entity.IsDeleted = true;
        entity.UpdatedBy = "api";
        entity.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
