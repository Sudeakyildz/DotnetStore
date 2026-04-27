using DotnetStore.Api.DTOs.Auth;
using DotnetStore.Api.Helpers;
using DotnetStore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("register")]
    [AllowAnonymous]
    public IActionResult RegisterDisabled()
    {
        return StatusCode(
            StatusCodes.Status403Forbidden,
            new { message = "Herkese açık kayıt kapalıdır. Size atanmış yönetici hesabı ile giriş yapın." });
    }

    [HttpPost("login")]
    [AllowAnonymous]
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
}
