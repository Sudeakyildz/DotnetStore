using DotnetStore.Api.Authorization;
using DotnetStore.Api.DTOs.Features;
using DotnetStore.Api.Helpers;
using DotnetStore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeaturesController : ControllerBase
{
    private readonly IFeatureService _features;

    public FeaturesController(IFeatureService features)
    {
        _features = features;
    }

    [HttpGet]
    [Authorize(Roles = AuthRoles.Admin + "," + AuthRoles.StaffFeatures + "," + AuthRoles.StaffPrices)]
    public async Task<ActionResult<IEnumerable<FeatureResponse>>> GetAll(CancellationToken ct)
    {
        return Ok(await _features.GetAllAsync(ct));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = AuthRoles.Admin + "," + AuthRoles.StaffFeatures + "," + AuthRoles.StaffPrices)]
    public async Task<ActionResult<FeatureResponse>> GetById(int id, CancellationToken ct)
    {
        var f = await _features.GetByIdAsync(id, ct);
        if (f is null) return NotFound();
        return Ok(f);
    }

    [HttpPost]
    [Authorize(Roles = AuthRoles.AdminOrStaffFeatures)]
    public async Task<IActionResult> Create([FromBody] FeatureCreateRequest dto, CancellationToken ct)
    {
        var r = await _features.CreateAsync(dto, ct);
        return r.ToActionResult(this, d => CreatedAtAction(nameof(GetById), new { id = d.Id }, d));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AuthRoles.AdminOrStaffFeatures)]
    public async Task<IActionResult> Update(int id, [FromBody] FeatureUpdateRequest dto, CancellationToken ct)
    {
        var r = await _features.UpdateAsync(id, dto, ct);
        return r.ToActionResult(this);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AuthRoles.AdminOrStaffFeatures)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var r = await _features.DeleteAsync(id, ct);
        return r.ToActionResult(this);
    }
}
