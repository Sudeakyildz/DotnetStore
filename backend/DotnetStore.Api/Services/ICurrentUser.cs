using StajDb.Models;

namespace DotnetStore.Api.Services;

public interface ICurrentUser
{
    int? UserId { get; }

    UserRole? Role { get; }
}
