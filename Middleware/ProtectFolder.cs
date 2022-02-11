using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GooDDevWebSite.Middleware
{
    public class ProtectFolder
    {
        PathString path { set; get; }
        string policy { set; get; }
        RequestDelegate next { set; get; }
        public ProtectFolder(RequestDelegate _next, ProtectFolderConfiguration config)
        {
            this.next = _next;
            this.policy = config.Policy;
            this.path = config.Path;
        }
        async public Task Invoke(HttpContext context, IAuthorizationService service)
        {
            if (context.Request.Path.StartsWithSegments(this.path))
            {
                var Authorization = await service.AuthorizeAsync(context.User, this.policy);
                if (Authorization.Failure != null)
                {
                    //var options = new AuthenticationOptions();
                    await context.ForbidAsync(this.policy);
                    return;
                }
            }
            await this.next(context);
        }
    }
    public record ProtectFolderConfiguration
    {
        public PathString Path { get; init; }
        public string Policy { get; init; }
        public ProtectFolderConfiguration (string policy, PathString path)
        {
            this.Path = path;
            this.Policy = policy;
        }
    }
    public static class MyExtensions
    {
        public static IApplicationBuilder UseProtectFolder( this IApplicationBuilder builder, ProtectFolderConfiguration options)
            => builder.UseMiddleware<ProtectFolder>(options);
    }
}
