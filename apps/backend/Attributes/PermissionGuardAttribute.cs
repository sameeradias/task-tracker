using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace backend.Attributes;

/// <summary>
/// Action filter attribute that checks user permissions before allowing access to an endpoint.
/// Applied to controllers or actions to enforce permission-based access control.
/// </summary>
/// <example>
/// [PermissionGuard("READ:TASK", "READ_OTHERS:TASK")]
/// public IActionResult GetTasks() { ... }
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class PermissionGuardAttribute : ActionFilterAttribute
{
    private readonly string[] _requiredPermissions;

    /// <summary>
    /// Creates a new permission guard that requires at least one of the specified permissions.
    /// </summary>
    /// <param name="requiredPermissions">One or more permissions required (OR logic - user needs at least one)</param>
    public PermissionGuardAttribute(params string[] requiredPermissions)
    {
        _requiredPermissions = requiredPermissions;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;

        // Check if user is authenticated
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication required." });
            return;
        }

        // Get user's role from claims
        var userRole = user.Claims.FirstOrDefault(c => c.Type == "user_role")?.Value;

        // Admin role bypasses all permission checks
        if (string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            base.OnActionExecuting(context);
            return;
        }

        // Get user's permissions from claims (comma-separated string)
        var permissionsClaim = user.Claims.FirstOrDefault(c => c.Type == "permissions")?.Value;

        if (string.IsNullOrEmpty(permissionsClaim))
        {
            context.Result = new ObjectResult(new { message = "You don't have permission to perform this action." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        var userPermissions = permissionsClaim.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Check if user has at least one of the required permissions (OR logic)
        var hasPermission = _requiredPermissions.Any(rp => userPermissions.Contains(rp));

        if (!hasPermission)
        {
            context.Result = new ObjectResult(new 
            { 
                message = "You don't have permission to perform this action. Please contact your administrator.",
                requiredPermissions = _requiredPermissions
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        base.OnActionExecuting(context);
    }
}