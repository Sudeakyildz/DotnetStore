using DotnetStore.Api.DTOs.Features;
using DotnetStore.Api.Services.Results;
using Microsoft.EntityFrameworkCore;
using StajDb;
using StajDb.Models;

namespace DotnetStore.Api.Services;

public sealed class FeatureService : IFeatureService
{
    private readonly DataContext _db;
    private readonly ICurrentUser _currentUser;

    public FeatureService(DataContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<FeatureResponse>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Features
            .AsNoTracking()
            .Where(f => !f.IsDeleted)
            .OrderBy(f => f.Name)
            .Select(f => ToResponse(f))
            .ToListAsync(ct);
    }

    public async Task<FeatureResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _db.Features
            .AsNoTracking()
            .Where(f => f.Id == id && !f.IsDeleted)
            .Select(f => ToResponse(f))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AppResult<FeatureResponse>> CreateAsync(FeatureCreateRequest dto, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(FeatureDataType), dto.DataType))
            return AppResult<FeatureResponse>.Fail("Geçersiz DataType.", 400);

        var name = dto.Name.Trim();
        var exists = await _db.Features.AnyAsync(x => !x.IsDeleted && x.Name == name, ct);
        if (exists)
            return AppResult<FeatureResponse>.Fail("Bu isimde bir özellik zaten var.", 409);

        var now = DateTime.UtcNow;
        var uid = _currentUser.UserId;
        var entity = new Feature
        {
            Name = name,
            DataType = (FeatureDataType)dto.DataType,
            IsDeleted = false,
            CreatedByUserId = uid,
            CreatedAt = now,
            UpdatedByUserId = uid,
            UpdatedAt = now,
        };

        _db.Features.Add(entity);
        await _db.SaveChangesAsync(ct);
        return AppResult<FeatureResponse>.Ok(ToResponse(entity));
    }

    public async Task<AppResult<Unit>> UpdateAsync(int id, FeatureUpdateRequest dto, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(FeatureDataType), dto.DataType))
            return AppResult<Unit>.Fail("Geçersiz DataType.", 400);

        var entity = await _db.Features.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null)
            return AppResult<Unit>.Fail("Bulunamadı.", 404);

        var name = dto.Name.Trim();
        var taken = await _db.Features.AnyAsync(x => !x.IsDeleted && x.Id != id && x.Name == name, ct);
        if (taken)
            return AppResult<Unit>.Fail("Bu isimde bir özellik zaten var.", 409);

        var now = DateTime.UtcNow;
        var uid = _currentUser.UserId;
        entity.Name = name;
        entity.DataType = (FeatureDataType)dto.DataType;
        entity.UpdatedByUserId = uid;
        entity.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);
        return AppResult<Unit>.Ok(Unit.Value);
    }

    public async Task<AppResult<Unit>> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _db.Features.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null)
            return AppResult<Unit>.Fail("Bulunamadı.", 404);

        var now = DateTime.UtcNow;
        entity.IsDeleted = true;
        entity.UpdatedByUserId = _currentUser.UserId;
        entity.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);
        return AppResult<Unit>.Ok(Unit.Value);
    }

    private static FeatureResponse ToResponse(Feature f) => new(
        f.Id,
        f.Name,
        (int)f.DataType,
        f.CreatedByUserId,
        f.UpdatedByUserId,
        f.CreatedAt,
        f.UpdatedAt);
}
