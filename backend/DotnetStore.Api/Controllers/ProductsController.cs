using DotnetStore.Api.Authorization;
using DotnetStore.Api.DTOs.Products;
using DotnetStore.Api.Helpers;
using DotnetStore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _products;

    public ProductsController(IProductService products)
    {
        _products = products;
    }

    [HttpGet]
    [Authorize(Roles = AuthRoles.Admin + "," + AuthRoles.StaffPrices + "," + AuthRoles.StaffFeatures)]
    public async Task<ActionResult<IEnumerable<ProductListItemDto>>> Search(
        [FromQuery] string? q,
        [FromQuery] int? categoryId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        CancellationToken ct)
    {
        var list = await _products.SearchAsync(q, categoryId, minPrice, maxPrice, ct);
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = AuthRoles.Admin + "," + AuthRoles.StaffPrices + "," + AuthRoles.StaffFeatures)]
    public async Task<ActionResult<ProductDetailDto>> GetById(int id, CancellationToken ct)
    {
        var p = await _products.GetByIdAsync(id, ct);
        if (p is null) return NotFound();
        return Ok(p);
    }

    [HttpPost]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] ProductCreateRequest dto, CancellationToken ct)
    {
        var r = await _products.CreateAsync(dto, ct);
        return r.ToActionResult(this, d => CreatedAtAction(nameof(GetById), new { id = d.Id }, d));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateRequest dto, CancellationToken ct)
    {
        var r = await _products.UpdateAsync(id, dto, ct);
        return r.ToActionResult(this);
    }

    [HttpPut("{id:int}/price")]
    [Authorize(Roles = AuthRoles.AdminOrStaffPrices)]
    public async Task<IActionResult> UpdatePrice(int id, [FromBody] ProductPriceOnlyUpdateRequest dto, CancellationToken ct)
    {
        var r = await _products.UpdatePriceOnlyAsync(id, dto.NewPrice, ct);
        return r.ToActionResult(this);
    }

    [HttpPut("{id:int}/feature-values")]
    [Authorize(Roles = AuthRoles.AdminOrStaffFeatures)]
    public async Task<IActionResult> UpdateFeatureValues(
        int id,
        [FromBody] ProductFeatureValuesUpdateRequest dto,
        CancellationToken ct)
    {
        var r = await _products.UpdateFeatureValuesOnlyAsync(id, dto.FeatureValues, ct);
        return r.ToActionResult(this);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var r = await _products.DeleteAsync(id, ct);
        return r.ToActionResult(this);
    }
}
