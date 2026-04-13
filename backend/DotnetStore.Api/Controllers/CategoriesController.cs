using DotnetStore.Api.DTOs.Categories;
using DotnetStore.Api.Helpers;
using DotnetStore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categories;

    public CategoriesController(ICategoryService categories)
    {
        _categories = categories;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll(CancellationToken ct)
    {
        var list = await _categories.GetAllAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> GetById(int id, CancellationToken ct)
    {
        var c = await _categories.GetByIdAsync(id, ct);
        if (c is null) return NotFound();
        return Ok(c);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryCreateRequest dto, CancellationToken ct)
    {
        var r = await _categories.CreateAsync(dto, ct);
        return r.ToActionResult(this, d => CreatedAtAction(nameof(GetById), new { id = d.Id }, d));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateRequest dto, CancellationToken ct)
    {
        var r = await _categories.UpdateAsync(id, dto, ct);
        return r.ToActionResult(this);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var r = await _categories.DeleteAsync(id, ct);
        return r.ToActionResult(this);
    }
}
