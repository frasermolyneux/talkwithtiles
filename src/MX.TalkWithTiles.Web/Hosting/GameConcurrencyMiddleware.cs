using MX.TalkWithTiles.Repository;

namespace MX.TalkWithTiles.Web.Hosting;

public sealed class GameConcurrencyMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (GameConcurrencyException)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsync("The game changed. Reload it before trying again.");
        }
    }
}
