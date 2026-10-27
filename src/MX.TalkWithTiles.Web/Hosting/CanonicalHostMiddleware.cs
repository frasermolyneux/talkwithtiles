using Microsoft.AspNetCore.Http.Extensions;

namespace MX.TalkWithTiles.Web.Hosting;

public sealed class CanonicalHostMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        if (string.Equals(context.Request.Host.Host, "www.talkwithtiles.com", StringComparison.OrdinalIgnoreCase))
        {
            var location = UriHelper.BuildAbsolute("https", new HostString("talkwithtiles.com"),
                context.Request.PathBase, context.Request.Path, context.Request.QueryString);
            context.Response.Redirect(location, permanent: true, preserveMethod: true);
            return Task.CompletedTask;
        }

        return next(context);
    }
}
