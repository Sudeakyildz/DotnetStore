using Microsoft.AspNetCore.Identity;

var hasher = new PasswordHasher<PwdUser>();
var hash = hasher.HashPassword(new PwdUser(), "admin123");
Console.WriteLine(hash);

sealed class PwdUser { public string Id { get; set; } = "1"; public string? UserName { get; set; } = "admin"; }
