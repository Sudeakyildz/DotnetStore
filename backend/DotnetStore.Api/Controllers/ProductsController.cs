using DotnetStore.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajDb;
using StajDb.Models;

namespace DotnetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly DataContext _db;

    public ProductsController(DataContext db)
    {
        _db = db;
    }

    /// <summary>Liste + arama/filtre: q (metin), categoryId, minPrice, maxPrice (aktif fiyata göre)</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductListItemDto>>> Search(
        [FromQuery] string? q,
        [FromQuery] int? categoryId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        CancellationToken ct)
    {
        IQueryable<Product> queryable = _db.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (categoryId.HasValue)
            queryable = queryable.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            queryable = queryable.Where(p =>
                p.Name.Contains(term) ||
                (p.Description != null && p.Description.Contains(term)));
        }

        if (minPrice.HasValue)
            queryable = queryable.Where(p =>
                p.Prices.Any(pp => pp.EndDate == null && pp.Price >= minPrice.Value));

        if (maxPrice.HasValue)
            queryable = queryable.Where(p =>
                p.Prices.Any(pp => pp.EndDate == null && pp.Price <= maxPrice.Value));

        var list = await queryable
            .OrderBy(p => p.Name)
            .Select(p => new ProductListItemDto(
                p.Id,
                p.CategoryId,
                p.Category != null ? p.Category.Name : null,
                p.Name,
                p.Stock,
                p.Status,
                p.Prices.Where(pp => pp.EndDate == null).Select(pp => (decimal?)pp.Price).FirstOrDefault(),
                p.ImageUrl))
            .ToListAsync(ct);

        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> GetById(int id, CancellationToken ct)
    {
        var p = await _db.Products
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new ProductDetailDto(
                x.Id,
                x.CategoryId,
                x.Category != null ? x.Category.Name : null,
                x.Name,
                x.Description,
                x.Stock,
                x.Status,
                x.ImageUrl,
                x.Prices.Where(pp => pp.EndDate == null).Select(pp => (decimal?)pp.Price).FirstOrDefault(),
                x.FeatureValues
                    .Select(fv => new ProductFeatureValueDto(
                        fv.FeatureId,
                        fv.Feature != null ? fv.Feature.Name : "",
                        fv.Value))
                    .ToList(),
                x.CreatedAt,
                x.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        if (p is null) return NotFound();
        return Ok(p);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDetailDto>> Create([FromBody] ProductCreateDto dto, CancellationToken ct)
    {
        var categoryOk = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId && !c.IsDeleted, ct);
        if (!categoryOk)
            return BadRequest(new { message = "Geçersiz ürün grubu (CategoryId)." });

        if (dto.FeatureValues is { Count: > 0 })
        {
            var featureIds = dto.FeatureValues.Select(f => f.FeatureId).Distinct().ToList();
            var validCount = await _db.Features.CountAsync(
                f => featureIds.Contains(f.Id) && !f.IsDeleted, ct);
            if (validCount != featureIds.Count)
                return BadRequest(new { message = "Geçersiz FeatureId var." });
        }

        var now = DateTime.UtcNow;
        var product = new Product
        {
            CategoryId = dto.CategoryId,
            Name = dto.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            Stock = dto.Stock,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "active" : dto.Status.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim(),
            IsDeleted = false,
            CreatedBy = "api",
            CreatedAt = now,
            UpdatedBy = "api",
            UpdatedAt = now
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        _db.ProductPrices.Add(new ProductPrice
        {
            ProductId = product.Id,
            Price = dto.Price,
            IsDiscount = false,
            StartDate = now,
            EndDate = null,
            CreatedBy = "api",
            CreatedAt = now,
            UpdatedBy = "api",
            UpdatedAt = now
        });

        if (dto.FeatureValues is { Count: > 0 })
        {
            foreach (var fv in dto.FeatureValues)
            {
                _db.ProductFeatures.Add(new ProductFeatureValue
                {
                    ProductId = product.Id,
                    FeatureId = fv.FeatureId,
                    Value = fv.Value.Trim(),
                    CreatedBy = "api",
                    CreatedAt = now,
                    UpdatedBy = "api",
                    UpdatedAt = now
                });
            }
        }

        await _db.SaveChangesAsync(ct);

        var detail = await GetDetailInternal(product.Id, ct);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, detail);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (product is null) return NotFound();

        var categoryOk = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId && !c.IsDeleted, ct);
        if (!categoryOk)
            return BadRequest(new { message = "Geçersiz ürün grubu (CategoryId)." });

        if (dto.FeatureValues is { Count: > 0 })
        {
            var featureIds = dto.FeatureValues.Select(f => f.FeatureId).Distinct().ToList();
            var validCount = await _db.Features.CountAsync(
                f => featureIds.Contains(f.Id) && !f.IsDeleted, ct);
            if (validCount != featureIds.Count)
                return BadRequest(new { message = "Geçersiz FeatureId var." });
        }

        var now = DateTime.UtcNow;
        product.CategoryId = dto.CategoryId;
        product.Name = dto.Name.Trim();
        product.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        product.Stock = dto.Stock;
        product.Status = string.IsNullOrWhiteSpace(dto.Status) ? "active" : dto.Status.Trim();
        product.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim();
        product.UpdatedBy = "api";
        product.UpdatedAt = now;

        if (dto.NewPrice.HasValue)
        {
            var active = await _db.ProductPrices
                .Where(pp => pp.ProductId == id && pp.EndDate == null)
                .ToListAsync(ct);
            foreach (var row in active)
            {
                row.EndDate = now;
                row.UpdatedBy = "api";
                row.UpdatedAt = now;
            }

            _db.ProductPrices.Add(new ProductPrice
            {
                ProductId = id,
                Price = dto.NewPrice.Value,
                IsDiscount = false,
                StartDate = now,
                EndDate = null,
                CreatedBy = "api",
                CreatedAt = now,
                UpdatedBy = "api",
                UpdatedAt = now
            });
        }

        if (dto.FeatureValues is not null)
        {
            var old = await _db.ProductFeatures.Where(x => x.ProductId == id).ToListAsync(ct);
            _db.ProductFeatures.RemoveRange(old);

            foreach (var fv in dto.FeatureValues)
            {
                _db.ProductFeatures.Add(new ProductFeatureValue
                {
                    ProductId = id,
                    FeatureId = fv.FeatureId,
                    Value = fv.Value.Trim(),
                    CreatedBy = "api",
                    CreatedAt = now,
                    UpdatedBy = "api",
                    UpdatedAt = now
                });
            }
        }

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (product is null) return NotFound();

        var now = DateTime.UtcNow;
        product.IsDeleted = true;
        product.UpdatedBy = "api";
        product.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<ProductDetailDto> GetDetailInternal(int id, CancellationToken ct)
    {
        var p = await _db.Products
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new ProductDetailDto(
                x.Id,
                x.CategoryId,
                x.Category != null ? x.Category.Name : null,
                x.Name,
                x.Description,
                x.Stock,
                x.Status,
                x.ImageUrl,
                x.Prices.Where(pp => pp.EndDate == null).Select(pp => (decimal?)pp.Price).FirstOrDefault(),
                x.FeatureValues
                    .Select(fv => new ProductFeatureValueDto(
                        fv.FeatureId,
                        fv.Feature != null ? fv.Feature.Name : "",
                        fv.Value))
                    .ToList(),
                x.CreatedAt,
                x.UpdatedAt))
            .FirstAsync(ct);

        return p;
    }
}
