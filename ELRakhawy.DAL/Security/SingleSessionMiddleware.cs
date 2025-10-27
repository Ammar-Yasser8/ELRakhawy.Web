using Elrakhawy.DAL.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;



public class SingleSessionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SingleSessionMiddleware> _logger;

    // Current system information
    private static string CURRENT_TIME => DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    private const string CURRENT_USER = "Ammar-Yasser8";

    public SingleSessionMiddleware(RequestDelegate next, ILogger<SingleSessionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDBContext dbContext)
    {
        try
        {
            // Check if user is authenticated
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sessionToken = context.User.FindFirst("SessionToken")?.Value;
                var userName = context.User.FindFirst(ClaimTypes.Name)?.Value ?? CURRENT_USER;

                _logger.LogDebug("🔐 Checking session for user {UserName} at {CurrentTime}", userName, CURRENT_TIME);

                if (int.TryParse(userIdClaim, out int userId))
                {
                    // Check if user exists and session is valid
                    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

                    if (user == null)
                    {
                        _logger.LogWarning("⚠️ User {UserId} not found in database at {CurrentTime}", userId, CURRENT_TIME);
                        await SignOutAndRedirect(context, "userNotFound");
                        return;
                    }

                    if (!string.IsNullOrEmpty(sessionToken) && user.CurrentSessionToken != sessionToken)
                    {
                        _logger.LogWarning("🚫 Invalid session token for user {UserName} (ID: {UserId}) at {CurrentTime}",
                            user.FullName, userId, CURRENT_TIME);
                        await SignOutAndRedirect(context, "sessionExpired");
                        return;
                    }

                    // Update session information
                    context.Session.SetString("UserName", user.FullName);
                    context.Session.SetString("UserEmail", user.Email);
                    context.Session.SetString("UserRole", user.Role.ToString());
                    context.Session.SetString("UserId", user.Id.ToString());
                    context.Session.SetString("LastActivity", CURRENT_TIME);

                    _logger.LogDebug("✅ Session validated for user {UserName} at {CurrentTime}", user.FullName, CURRENT_TIME);
                }
            }
            else
            {
                // For non-authenticated requests, check if accessing protected resources
                var path = context.Request.Path.Value?.ToLower();
                var protectedPaths = new[] { "/manufacturers", "/user", "/admin" };

                if (protectedPaths.Any(p => path?.StartsWith(p) == true))
                {
                    _logger.LogInformation("🔒 Redirecting unauthenticated request to {Path} at {CurrentTime}", path, CURRENT_TIME);
                    context.Response.Redirect("/Auth/Login?returnUrl=" + Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
                    return;
                }
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in SingleSessionMiddleware at {CurrentTime}", CURRENT_TIME);
            await _next(context);
        }
    }

    private async Task SignOutAndRedirect(HttpContext context, string reason)
    {
        try
        {
            // Clear session
            context.Session.Clear();

            // Sign out
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("🔓 User signed out due to {Reason} at {CurrentTime}", reason, CURRENT_TIME);

            // Redirect to login with reason
            context.Response.Redirect($"/Auth/Login?sessionExpired=true&reason={reason}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during sign out at {CurrentTime}", CURRENT_TIME);
            context.Response.Redirect("/Auth/Login");
        }
    }
}

// Extension method to register the middleware
public static class SingleSessionMiddlewareExtensions
{
    public static IApplicationBuilder UseSingleSessionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SingleSessionMiddleware>();
    }
}