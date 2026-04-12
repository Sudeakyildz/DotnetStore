namespace DotnetStore.Api.Services.Results;

/// <summary>Boş başarı sonucu (204 No Content).</summary>
public readonly struct Unit
{
    public static Unit Value => default;
}
