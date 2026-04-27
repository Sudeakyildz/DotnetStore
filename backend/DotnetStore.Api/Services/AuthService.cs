using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotnetStore.Api.DTOs.Auth;
using DotnetStore.Api.Infrastructure;
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
    private readonly ICurrentUser _currentUser;
    private readonly PasswordHasher<StoreUser> _hasher = new();

    public AuthService(DataContext db, IOptions<JwtOptions> jwt, ICurrentUser currentUser)
    {
        _db = db;
        _jwt = jwt.Value;
        _currentUser = currentUser;
    }

    public async Task<AppResult<Unit>> LogoutAsync(CancellationToken ct)
    {
        var id = _currentUser.UserId;
        if (id is null)
            return AppResult<Unit>.Fail("Oturum yok.", 401);

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = id.Value,
            Action = AuditActions.Logout,
            Details = null,
            CreatedAtUtc = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync(ct);
        return AppResult<Unit>.Ok(Unit.Value);
    }

    public async Task<AppResult<LoginResponse>> RegisterAsync(RegisterRequest dto, CancellationToken ct)
    {
        var username = dto.Username.Trim();
        var email = NormalizeEmail(dto.Email);
        if (string.IsNullOrEmpty(username))
            return AppResult<LoginResponse>.Fail("Kullanıcı adı gerekli.", 400);
        if (string.IsNullOrEmpty(email))
            return AppResult<LoginResponse>.Fail("E-posta gerekli.", 400);

        if (await _db.Users.AnyAsync(u => u.UserName == username, ct))
            return AppResult<LoginResponse>.Fail("Bu kullanıcı adı alınmış.", 409);
        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            return AppResult<LoginResponse>.Fail("Bu e-posta kayıtlı.", 409);

        var now = DateTime.UtcNow;
        var user = new StoreUser
        {
            UserName = username,
            Email = email,
            Role = UserRole.StaffCategories,
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
        var email = NormalizeEmail(dto.Email);
        if (string.IsNullOrEmpty(email))
            return AppResult<LoginResponse>.Fail("E-posta gerekli.", 400);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null)
            return AppResult<LoginResponse>.Fail("Geçersiz e-posta veya şifre.", 401);

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (verify == PasswordVerificationResult.Failed)
            return AppResult<LoginResponse>.Fail("Geçersiz e-posta veya şifre.", 401);

        var now = DateTime.UtcNow;
        user.FirstLoginAt ??= now;
        user.LastLoginAt = now;
        user.UpdatedAt = now;
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = AuditActions.Login,
            Details = null,
            CreatedAtUtc = now,
        });
        await _db.SaveChangesAsync(ct);

        var token = CreateToken(user);
        return AppResult<LoginResponse>.Ok(token);
    }

    public async Task<AppResult<MeResponse>> GetMeAsync(CancellationToken ct)
    {
        var id = _currentUser.UserId;
        if (id is null)
            return AppResult<MeResponse>.Fail("Oturum yok.", 401);

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id.Value, ct);
        if (user is null)
            return AppResult<MeResponse>.Fail("Kullanıcı bulunamadı.", 404);

        return AppResult<MeResponse>.Ok(new MeResponse(
            user.Id,
            user.Email,
            user.UserName,
            user.Role.ToString(),
            user.FirstLoginAt,
            user.LastLoginAt,
            user.CreatedAt));
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

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
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
        };

        var jwt = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new LoginResponse(
            token,
            "Bearer",
            (int)(expires - DateTime.UtcNow).TotalSeconds,
            user.Email,
            user.UserName,
            user.Role.ToString());
    }
}
