using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DymanicRolesBasedAuthorization.Data
{
    public class DynamicRoleHandler
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DynamicRoleHandler(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.Request.Path.Value;
            var method = context.Request.Method;

            string[] excludedEndpoints = new[] {
            "/api/account/signin",
            "/api/account/signup",
            "/api/account/forgotpassword",
            "/swagger"
            };

            if (excludedEndpoints.Any(e => endpoint.StartsWith(e, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            if (!context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Authentication required.");
                return;
            }

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: User ID not found in token.");
                return;
            }

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUsers>>();

                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized: User not found.");
                    return;
                }

                var roles = await userManager.GetRolesAsync(user);

                if (roles.Contains("Admin") || roles.Contains("SuperAdmin"))
                {
                    await _next(context);
                    return;
                }

                bool hasExactPermission = await dbContext.tblRoleApiPermissions
    .AnyAsync(p => roles.Contains(p.RoleName) &&
                   p.Endpoint.ToLower() == endpoint.ToLower() &&
                   p.HttpMethod.ToLower() == method.ToLower());


                if (hasExactPermission)
                {
                    await _next(context);
                    return;
                }

                bool hasWildcardPermission = await dbContext.tblRoleApiPermissions
    .AnyAsync(p => roles.Contains(p.RoleName) &&
                   p.HttpMethod.ToLower() == method.ToLower() &&
                   p.Endpoint.EndsWith("/*") &&
                   endpoint.ToLower().StartsWith(p.Endpoint.Substring(0, p.Endpoint.Length - 2).ToLower()));


                if (hasWildcardPermission)
                {
                    await _next(context);
                    return;
                }

                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden: You do not have permission to access this resource.");
            }
        }
    }
}