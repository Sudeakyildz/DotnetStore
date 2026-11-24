using DotnetStore.Api.Authorization;
using DotnetStore.Api.DTOs.Orders;
using DotnetStore.Api.DTOs.Store;
using DotnetStore.Api.Helpers;
using DotnetStore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetStore.Api.Controllers;

/// <summary>Herkese açık vitrin uçları (giriş gerekmez).</summary>
[ApiController]
[Route("api/store")]
[AllowAnonymous]
public class StoreController : ControllerBase
{
    private readonly IStoreCatalogService _catalog;

    public StoreController(IStoreCatalogService catalog) => _catalog = catalog;

    [HttpGet("products")]
    public async Task<ActionResult<PagedResult<StoreProductListItem>>> Products(
        [FromQuery] string? q,
        [FromQuery] int? categoryId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool inStockOnly,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default) =>
        Ok(await _catalog.SearchAsync(q, categoryId, minPrice, maxPrice, inStockOnly, sort, page, pageSize, ct));

    [HttpGet("products/{id:int}")]
    public async Task<ActionResult<StoreProductDetail>> Product(int id, CancellationToken ct)
    {
        var p = await _catalog.GetAsync(id, ct);
        return p is null ? NotFound(new { message = "Ürün bulunamadı." }) : Ok(p);
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<StoreCategoryDto>>> Categories(CancellationToken ct) =>
        Ok(await _catalog.CategoriesAsync(ct));
}

[ApiController]
[Route("api/cart")]
[Authorize(Roles = AuthRoles.Musteri)]
public class CartController : ControllerBase
{
    private readonly ICartService _cart;
    private readonly ICurrentUser _user;

    public CartController(ICartService cart, ICurrentUser user)
    {
        _cart = cart;
        _user = user;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> Get(CancellationToken ct) =>
        Ok(await _cart.GetAsync(_user.UserId!.Value, ct));

    [HttpPost("items")]
    public async Task<IActionResult> Add([FromBody] AddCartItemRequest dto, CancellationToken ct) =>
        (await _cart.AddAsync(_user.UserId!.Value, dto.ProductId, dto.Quantity, ct)).ToActionResult(this, Ok);

    [HttpPut("items/{productId:int}")]
    public async Task<IActionResult> SetQuantity(int productId, [FromBody] SetCartQuantityRequest dto, CancellationToken ct) =>
        (await _cart.SetQuantityAsync(_user.UserId!.Value, productId, dto.Quantity, ct)).ToActionResult(this, Ok);

    [HttpDelete("items/{productId:int}")]
    public async Task<ActionResult<CartDto>> Remove(int productId, CancellationToken ct) =>
        Ok(await _cart.RemoveAsync(_user.UserId!.Value, productId, ct));
}

[ApiController]
[Route("api/favorites")]
[Authorize(Roles = AuthRoles.Musteri)]
public class FavoritesController : ControllerBase
{
    private readonly ICartService _cart;
    private readonly IStoreCatalogService _catalog;
    private readonly ICurrentUser _user;

    public FavoritesController(ICartService cart, IStoreCatalogService catalog, ICurrentUser user)
    {
        _cart = cart;
        _catalog = catalog;
        _user = user;
    }

    [HttpGet("ids")]
    public async Task<ActionResult<IReadOnlyList<int>>> Ids(CancellationToken ct) =>
        Ok(await _cart.FavoriteIdsAsync(_user.UserId!.Value, ct));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StoreProductListItem>>> List(CancellationToken ct)
    {
        var ids = await _cart.FavoriteIdsAsync(_user.UserId!.Value, ct);
        var products = await _catalog.ByIdsAsync(ids, ct);
        var order = ids.Select((id, i) => (id, i)).ToDictionary(x => x.id, x => x.i);
        return Ok(products.OrderBy(p => order[p.Id]).ToList());
    }

    [HttpPut("{productId:int}")]
    public async Task<IActionResult> Add(int productId, CancellationToken ct) =>
        (await _cart.AddFavoriteAsync(_user.UserId!.Value, productId, ct)).ToActionResult(this);

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Remove(int productId, CancellationToken ct)
    {
        await _cart.RemoveFavoriteAsync(_user.UserId!.Value, productId, ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/my")]
[Authorize(Roles = AuthRoles.Musteri)]
public class MyOrdersController : ControllerBase
{
    private readonly IOrderService _orders;
    private readonly ICheckoutService _checkout;
    private readonly ICurrentUser _user;

    public MyOrdersController(IOrderService orders, ICheckoutService checkout, ICurrentUser user)
    {
        _orders = orders;
        _checkout = checkout;
        _user = user;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest dto, CancellationToken ct) =>
        (await _checkout.CheckoutAsync(_user.UserId!.Value, dto, ct)).ToActionResult(this, Ok);

    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyList<OrderListItemDto>>> List(CancellationToken ct) =>
        Ok(await _orders.ListAsync(_user.UserId!.Value, ct));

    [HttpGet("orders/{id:int}")]
    public async Task<ActionResult<OrderDetailDto>> Get(int id, CancellationToken ct)
    {
        var o = await _orders.GetByIdAsync(id, _user.UserId!.Value, ct);
        return o is null ? NotFound(new { message = "Sipariş bulunamadı." }) : Ok(o);
    }

    [HttpPost("orders/{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct) =>
        (await _checkout.CancelOwnOrderAsync(_user.UserId!.Value, id, ct)).ToActionResult(this);
}
