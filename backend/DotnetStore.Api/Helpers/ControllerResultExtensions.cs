using DotnetStore.Api.Services.Results;
using Microsoft.AspNetCore.Mvc;

namespace DotnetStore.Api.Helpers;

public static class ControllerResultExtensions
{
    public static IActionResult ToActionResult<T>(this AppResult<T> result, ControllerBase c, Func<T, IActionResult> onSuccess)
    {
        if (!result.IsSuccess)
            return c.StatusCode(result.StatusCode, new { message = result.ErrorMessage });
        return onSuccess(result.Data!);
    }

    public static IActionResult ToActionResult(this AppResult<Unit> result, ControllerBase c)
    {
        if (!result.IsSuccess)
            return c.StatusCode(result.StatusCode, new { message = result.ErrorMessage });
        return c.NoContent();
    }
}
