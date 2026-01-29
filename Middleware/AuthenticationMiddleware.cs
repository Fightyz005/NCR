using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using NCRManagementSystem.Services.Interfaces;

namespace NCRManagementSystem.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Skip authentication for certain paths
                if (ShouldSkipAuthentication(context.Request.Path))
                {
                    await _next(context);
                    return;
                }

                // Check if user is authenticated
                if (!context.User.Identity?.IsAuthenticated == true)
                {
                    // If it's an API request, return JSON response
                    if (IsApiRequest(context.Request))
                    {
                        await HandleUnauthenticatedApiRequest(context);
                        return;
                    }

                    // For regular requests, redirect to login
                    await HandleUnauthenticatedWebRequest(context);
                    return;
                }

                // Validate user session and update last activity
                if (context.User.Identity.IsAuthenticated)
                {
                    await ValidateAndUpdateUserSession(context);
                }

                // Check if user has required role for the current path
                if (!await HasRequiredRole(context))
                {
                    await HandleUnauthorizedRequest(context);
                    return;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AuthenticationMiddleware");
                await HandleAuthenticationError(context);
            }
        }

        private static bool ShouldSkipAuthentication(PathString path)
        {
            var publicPaths = new[]
            {
                "/",
                "/Home",
                "/Auth/Login",
                "/Auth/Logout",
                "/Auth/AccessDenied",
                "/css",
                "/js",
                "/lib",
                "/images",
                "/favicon.ico",
                "/robots.txt"
            };

            return publicPaths.Any(publicPath =>
                path.StartsWithSegments(publicPath, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsApiRequest(HttpRequest request)
        {
            return request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) ||
                   request.Headers.Accept.ToString().Contains("application/json") ||
                   request.ContentType?.Contains("application/json") == true;
        }

        private async Task HandleUnauthenticatedApiRequest(HttpContext context)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                message = "Authentication required",
                redirectUrl = "/Auth/Login"
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }

        private async Task HandleUnauthenticatedWebRequest(HttpContext context)
        {
            var returnUrl = context.Request.Path + context.Request.QueryString;
            var loginUrl = $"/Auth/Login?returnUrl={Uri.EscapeDataString(returnUrl)}";

            context.Response.Redirect(loginUrl);
            await Task.CompletedTask;
        }

        private async Task ValidateAndUpdateUserSession(HttpContext context)
        {
            try
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var userId))
                {
                    // You could add additional session validation here
                    // For example, check if user is still active in database

                    // Update user's last activity (if you want to track this)
                    context.Items["UserId"] = userId;
                    context.Items["LastActivity"] = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate user session for user {UserId}",
                    context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            }

            await Task.CompletedTask;
        }

        private async Task<bool> HasRequiredRole(HttpContext context)
        {
            try
            {
                var path = context.Request.Path.Value?.ToLowerInvariant();
                var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(userRole))
                    return true; // Allow if we can't determine requirements

                // Define role requirements for different paths
                var roleRequirements = new Dictionary<string, string[]>
                {
                    { "/dashboard", new[] { "User", "QA", "Manager", "Admin" } },
                    { "/ncr", new[] { "User", "QA", "Manager", "Admin" } },
                    { "/qa", new[] { "QA", "Manager", "Admin" } },
                    { "/supplier", new[] { "User" } },
                    { "/manager", new[] { "Manager", "Admin" } },
                    { "/report", new[] { "User", "QA", "Manager", "Admin" } },
                    { "/admin", new[] { "Admin" } }
                };

                foreach (var requirement in roleRequirements)
                {
                    if (path.StartsWith(requirement.Key))
                    {
                        return requirement.Value.Contains(userRole, StringComparer.OrdinalIgnoreCase);
                    }
                }

                // Default: allow access if no specific requirement found
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role requirements");
                return false; // Deny access on error
            }
        }

        private async Task HandleUnauthorizedRequest(HttpContext context)
        {
            if (IsApiRequest(context.Request))
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    success = false,
                    message = "Access denied. Insufficient permissions.",
                    requiredRole = GetRequiredRoleForPath(context.Request.Path)
                };

                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
            }
            else
            {
                context.Response.Redirect("/Auth/AccessDenied");
            }
        }

        private async Task HandleAuthenticationError(HttpContext context)
        {
            if (IsApiRequest(context.Request))
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    success = false,
                    message = "Authentication error occurred"
                };

                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
            }
            else
            {
                context.Response.Redirect("/Auth/Login?error=authentication_error");
            }
        }

        private static string GetRequiredRoleForPath(PathString path)
        {
            var pathValue = path.Value?.ToLowerInvariant();

            return pathValue switch
            {
                var p when p?.StartsWith("/qa") == true => "QA, Manager, or Admin",
                var p when p?.StartsWith("/manager") == true => "Manager or Admin",
                var p when p?.StartsWith("/supplier") == true => "User",
                var p when p?.StartsWith("/admin") == true => "Admin",
                _ => "User, QA, Manager, or Admin"
            };
        }
    }

    // Extension method to add the middleware
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }

    // Additional helper class for session management
    public class SessionManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SessionManager> _logger;

        public SessionManager(IHttpContextAccessor httpContextAccessor, ILogger<SessionManager> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public int? GetCurrentUserId()
        {
            try
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return int.TryParse(userIdClaim, out var userId) ? userId : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user ID");
                return null;
            }
        }

        public string? GetCurrentUserRole()
        {
            try
            {
                return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user role");
                return null;
            }
        }

        public string? GetCurrentUserName()
        {
            try
            {
                return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.GivenName)?.Value ??
                       _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user name");
                return null;
            }
        }

        public bool IsAuthenticated()
        {
            try
            {
                return _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authentication status");
                return false;
            }
        }

        public bool HasRole(string role)
        {
            try
            {
                return _httpContextAccessor.HttpContext?.User.IsInRole(role) == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user role {Role}", role);
                return false;
            }
        }

        public bool HasAnyRole(params string[] roles)
        {
            try
            {
                return roles.Any(role => _httpContextAccessor.HttpContext?.User.IsInRole(role) == true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking multiple user roles");
                return false;
            }
        }

        public void UpdateLastActivity()
        {
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    _httpContextAccessor.HttpContext.Items["LastActivity"] = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last activity");
            }
        }

        public DateTime? GetLastActivity()
        {
            try
            {
                return _httpContextAccessor.HttpContext?.Items["LastActivity"] as DateTime?;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last activity");
                return null;
            }
        }
    }

    // Security helper for additional authentication features
    public static class SecurityHelper
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public static bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                return false;

            // Check for at least one uppercase, one lowercase, and one number
            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);

            return hasUpper && hasLower && hasDigit;
        }

        public static string GenerateSecureToken(int length = 32)
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailRegex = new System.Text.RegularExpressions.Regex(
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }
    }
}

// Update Program.cs to register the SessionManager
/*
Add this to your Program.cs in the service registration section:

// Add HTTP Context Accessor for SessionManager
builder.Services.AddHttpContextAccessor();

// Add SessionManager
builder.Services.AddScoped<SessionManager>();

// And replace the existing middleware registration with:
app.UseCustomAuthentication(); // Instead of app.UseMiddleware<AuthenticationMiddleware>();
*/