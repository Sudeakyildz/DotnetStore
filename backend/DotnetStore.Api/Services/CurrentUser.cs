using System.Security.Claims;
using StajDb.Models;

namespace DotnetStore.Api.Services;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var sub = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(sub, out var id) ? id : null;
        }
    }

    public UserRole? Role
    {
        get
        {
            var raw = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(raw, ignoreCase: true, out var r) ? r : null;
        }
    }
}
