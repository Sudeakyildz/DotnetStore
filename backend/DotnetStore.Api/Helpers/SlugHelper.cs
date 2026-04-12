namespace DotnetStore.Api.Helpers;

public static class SlugHelper
{
    public static string? FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var parts = name.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? null : string.Join('-', parts);
    }
}
