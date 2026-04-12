using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotnetStore.Api.DTOs.Auth;
using DotnetStore.Api.Options;
using DotnetStore.Api.Services.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StajDb;
using StajDb.Models;

namespace DotnetStore.Api.Services;

public sealed class AuthService : IAuthService
{
    private readonly DataContext _db;
    private readonly JwtOptions _jwt;
    private readonly PasswordHasher<StoreUser> _hasher = new();

    public AuthService(DataContext db, IOptions<JwtOptions> jwt)
    {
        _db = db;
        _jwt = jwt.Value;
    }

    public async Task<AppResult<LoginResponse>> RegisterAsync(RegisterRequest dto, CancellationToken ct)
    {
        var username = dto.Username.Trim();
        if (string.IsNullOrEmpty(username))
            return AppResult<LoginResponse>.Fail("Kullanıcı adı gerekli.", 400);

        if (await _db.Users.AnyAsync(u => u.UserName == username, ct))
            return AppResult<LoginResponse>.Fail("Bu kullanıcı adı alınmış.", 409);

        var now = DateTime.UtcNow;
        var user = new StoreUser
        {
            UserName = username,
            PasswordHash = "",
            CreatedAt = now,
            UpdatedAt = now,
        };
        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        var token = CreateToken(user);
        return AppResult<LoginResponse>.Ok(token);
    }

    public async Task<AppResult<LoginResponse>> LoginAsync(LoginRequest dto, CancellationToken ct)
    {
        var username = dto.Username.Trim();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == username, ct);
        if (user is null)
            return AppResult<LoginResponse>.Fail("Geçersiz kullanıcı adı veya şifre.", 401);

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (verify == PasswordVerificationResult.Failed)
            return AppResult<LoginResponse>.Fail("Geçersiz kullanıcı adı veya şifre.", 401);

        var token = CreateToken(user);
        return AppResult<LoginResponse>.Ok(token);
    }

    private LoginResponse CreateToken(StoreUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiresMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
        };

        var jwt = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new LoginResponse(token, "Bearer", (int)(expires - DateTime.UtcNow).TotalSeconds);
    }
}
