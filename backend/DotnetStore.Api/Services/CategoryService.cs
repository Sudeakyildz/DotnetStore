using DotnetStore.Api.DTOs.Categories;
using DotnetStore.Api.Helpers;
using DotnetStore.Api.Infrastructure;
using DotnetStore.Api.Services.Results;
using Microsoft.EntityFrameworkCore;
using StajDb;
using StajDb.Models;

namespace DotnetStore.Api.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly DataContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _audit;

    public CategoryService(DataContext db, ICurrentUser currentUser, IAuditService audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Categories
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .Select(c => ToResponse(c))
            .ToListAsync(ct);
    }

    public async Task<CategoryResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _db.Categories
            .AsNoTracking()
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => ToResponse(c))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AppResult<CategoryResponse>> CreateAsync(CategoryCreateRequest dto, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var slug = string.IsNullOrWhiteSpace(dto.Slug) ? SlugHelper.FromName(dto.Name) : dto.Slug.Trim();

        if (!string.IsNullOrEmpty(slug))
        {
            var taken = await _db.Categories.AnyAsync(x => !x.IsDeleted && x.Slug == slug, ct);
            if (taken)
                return AppResult<CategoryResponse>.Fail("Bu slug zaten kullanılıyor.", 409);
        }

        var uid = _currentUser.UserId;
        var entity = new Category
        {
            Name = dto.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim(),
            Slug = string.IsNullOrEmpty(slug) ? null : slug,
            IsDeleted = false,
            CreatedByUserId = uid,
            CreatedAt = now,
            UpdatedByUserId = uid,
            UpdatedAt = now,
        };

        _db.Categories.Add(entity);
        await _db.SaveChangesAsync(ct);

        if (uid is int u1)
            await _audit.LogAsync(u1, AuditActions.CategoryCreate, $"Grup #{entity.Id}: {entity.Name}", ct);

        return AppResult<CategoryResponse>.Ok(ToResponse(entity));
    }

    public async Task<AppResult<Unit>> UpdateAsync(int id, CategoryUpdateRequest dto, CancellationToken ct)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null)
            return AppResult<Unit>.Fail("Bulunamadı.", 404);

        var slug = string.IsNullOrWhiteSpace(dto.Slug) ? SlugHelper.FromName(dto.Name) : dto.Slug.Trim();
        if (!string.IsNullOrEmpty(slug))
        {
            var taken = await _db.Categories.AnyAsync(x => !x.IsDeleted && x.Id != id && x.Slug == slug, ct);
            if (taken)
                return AppResult<Unit>.Fail("Bu slug zaten kullanılıyor.", 409);
        }

        var now = DateTime.UtcNow;
        var uid = _currentUser.UserId;
        entity.Name = dto.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        entity.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim();
        entity.Slug = string.IsNullOrEmpty(slug) ? null : slug;
        entity.UpdatedByUserId = uid;
        entity.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);

        if (uid is int u2)
            await _audit.LogAsync(u2, AuditActions.CategoryUpdate, $"Grup #{id}: {entity.Name}", ct);

        return AppResult<Unit>.Ok(Unit.Value);
    }

    public async Task<AppResult<Unit>> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null)
            return AppResult<Unit>.Fail("Bulunamadı.", 404);

        var now = DateTime.UtcNow;
        var name = entity.Name;
        entity.IsDeleted = true;
        entity.UpdatedByUserId = _currentUser.UserId;
        entity.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        if (_currentUser.UserId is int u3)
            await _audit.LogAsync(u3, AuditActions.CategoryDelete, $"Grup #{id}: {name}", ct);

        return AppResult<Unit>.Ok(Unit.Value);
    }

    private static CategoryResponse ToResponse(Category c) => new(
        c.Id,
        c.Name,
        c.Description,
        c.ImageUrl,
        c.Slug,
        c.CreatedByUserId,
        c.UpdatedByUserId,
        c.CreatedAt,
        c.UpdatedAt);
}
