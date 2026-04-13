using DotnetStore.Api.DTOs.Auth;
using DotnetStore.Api.Helpers;
using DotnetStore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest body, CancellationToken ct)
    {
        var r = await _auth.RegisterAsync(body, ct);
        return r.ToActionResult(this, d => Ok(d));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest body, CancellationToken ct)
    {
        var r = await _auth.LoginAsync(body, ct);
        return r.ToActionResult(this, d => Ok(d));
    }
}
