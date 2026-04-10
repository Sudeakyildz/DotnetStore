using DotnetStore.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DotnetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>Demo giriş. Frontend entegrasyonu için basit token döner (gerçek JWT değildir).</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequestDto body)
    {
        var expectedUser = _configuration["Auth:DemoUsername"];
        var expectedPass = _configuration["Auth:DemoPassword"];

        if (string.IsNullOrEmpty(expectedUser) || string.IsNullOrEmpty(expectedPass))
            return Problem("Sunucu yapılandırması eksik: Auth.");

        if (!string.Equals(body.Username, expectedUser, StringComparison.Ordinal)
            || !string.Equals(body.Password, expectedPass, StringComparison.Ordinal))
            return Unauthorized(new { message = "Geçersiz kullanıcı adı veya şifre." });

        var payload = $"demo|{body.Username}|{DateTime.UtcNow:O}";
        var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));

        return Ok(new LoginResponseDto(token, "Bearer", 86400));
    }
}
