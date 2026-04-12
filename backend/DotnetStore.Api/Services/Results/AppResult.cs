namespace DotnetStore.Api.Services.Results;

public sealed class AppResult<T>
{
    public T? Data { get; private init; }
    public string? ErrorMessage { get; private init; }
    public int StatusCode { get; private init; }

    public bool IsSuccess => ErrorMessage is null;

    public static AppResult<T> Ok(T data) => new() { Data = data, StatusCode = 200 };

    public static AppResult<T> Fail(string message, int statusCode = 400) =>
        new() { ErrorMessage = message, StatusCode = statusCode };
}
