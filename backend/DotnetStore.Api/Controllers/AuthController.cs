using DotnetStore.Api.DTOs.Auth;
using DotnetStore.Api.Helpers;
using DotnetStore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DotnetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>Mağaza müşterisi kaydı (rol her zaman Musteri).</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest body, CancellationToken ct)
    {
        var r = await _auth.RegisterAsync(body, ct);
        return r.ToActionResult(this, d => Ok(d));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest body, CancellationToken ct)
    {
        var r = await _auth.LoginAsync(body, ct);
        return r.ToActionResult(this, d => Ok(d));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var r = await _auth.LogoutAsync(ct);
        return r.ToActionResult(this);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var r = await _auth.GetMeAsync(ct);
        return r.ToActionResult(this, d => Ok(d));
    }

    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] DotnetStore.Api.DTOs.Store.ChangePasswordRequest body, CancellationToken ct)
    {
        var r = await _auth.ChangePasswordAsync(body.CurrentPassword, body.NewPassword, ct);
        return r.ToActionResult(this);
    }
}
