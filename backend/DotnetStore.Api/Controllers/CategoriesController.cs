using DotnetStore.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajDb;
using StajDb.Models;

namespace DotnetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly DataContext _db;

    public CategoriesController(DataContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAll(CancellationToken ct)
    {
        var list = await _db.Categories
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponseDto(
                c.Id,
                c.Name,
                c.Description,
                c.ImageUrl,
                c.Slug,
                c.CreatedAt,
                c.UpdatedAt))
            .ToListAsync(ct);

        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponseDto>> GetById(int id, CancellationToken ct)
    {
        var c = await _db.Categories
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new CategoryResponseDto(
                x.Id,
                x.Name,
                x.Description,
                x.ImageUrl,
                x.Slug,
                x.CreatedAt,
                x.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        if (c is null) return NotFound();
        return Ok(c);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> Create([FromBody] CategoryCreateDto dto, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var slug = string.IsNullOrWhiteSpace(dto.Slug) ? SlugFromName(dto.Name) : dto.Slug.Trim();

        if (!string.IsNullOrEmpty(slug))
        {
            var slugTaken = await _db.Categories.AnyAsync(
                x => !x.IsDeleted && x.Slug == slug, ct);
            if (slugTaken)
                return Conflict(new { message = "Bu slug zaten kullanılıyor." });
        }

        var entity = new Category
        {
            Name = dto.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim(),
            Slug = string.IsNullOrEmpty(slug) ? null : slug,
            IsDeleted = false,
            CreatedBy = "api",
            CreatedAt = now,
            UpdatedBy = "api",
            UpdatedAt = now
        };

        _db.Categories.Add(entity);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new CategoryResponseDto(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.ImageUrl,
            entity.Slug,
            entity.CreatedAt,
            entity.UpdatedAt));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto, CancellationToken ct)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null) return NotFound();

        var slug = string.IsNullOrWhiteSpace(dto.Slug) ? SlugFromName(dto.Name) : dto.Slug.Trim();
        if (!string.IsNullOrEmpty(slug))
        {
            var slugTaken = await _db.Categories.AnyAsync(
                x => !x.IsDeleted && x.Id != id && x.Slug == slug, ct);
            if (slugTaken)
                return Conflict(new { message = "Bu slug zaten kullanılıyor." });
        }

        var now = DateTime.UtcNow;
        entity.Name = dto.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        entity.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim();
        entity.Slug = string.IsNullOrEmpty(slug) ? null : slug;
        entity.UpdatedBy = "api";
        entity.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Soft delete: IsDeleted = true</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null) return NotFound();

        var now = DateTime.UtcNow;
        entity.IsDeleted = true;
        entity.UpdatedBy = "api";
        entity.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static string? SlugFromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var parts = name.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? null : string.Join('-', parts);
    }
}
