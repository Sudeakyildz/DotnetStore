using DotnetStore.Api.Authorization;
using DotnetStore.Api.DTOs.Orders;
using DotnetStore.Api.Helpers;
using DotnetStore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders) => _orders = orders;

    [HttpGet]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<ActionResult<IReadOnlyList<OrderListItemDto>>> List(CancellationToken ct) =>
        Ok(await _orders.ListAsync(ct));

    [HttpGet("{id:int}")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<ActionResult<OrderDetailDto>> GetById(int id, CancellationToken ct)
    {
        var o = await _orders.GetByIdAsync(id, ct);
        if (o is null) return NotFound();
        return Ok(o);
    }

    [HttpPost]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] OrderCreateRequest dto, CancellationToken ct)
    {
        var r = await _orders.CreateAsync(dto, ct);
        return r.ToActionResult(
            this,
            d => CreatedAtAction(nameof(GetById), new { id = d.Id }, d));
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] OrderStatusUpdateRequest dto,
        CancellationToken ct) =>
        (await _orders.UpdateStatusAsync(id, dto, ct)).ToActionResult(this);
}
