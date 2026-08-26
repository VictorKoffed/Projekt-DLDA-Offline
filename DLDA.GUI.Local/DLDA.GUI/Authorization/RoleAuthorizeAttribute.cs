using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DLDA.GUI.Authorization
{
    /// <summary>
    /// Custom authorization filter attribute enforcing role-based access control (RBAC) 
    /// by inspecting active session state claims against permitted route roles.
    /// </summary>
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles; // Stores normalized lowercase role identifiers permitted to access the protected resource

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleAuthorizeAttribute"/> class with specified allowed roles.
        /// </summary>
        /// <param name="roles">The list of authorized security roles.</param>
        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles.Select(r => r.ToLower()).ToArray(); // Normalizes incoming role definitions to lowercase to ensure case-insensitive string comparisons during authorization checks
        }

        /// <summary>
        /// Evaluates user session credentials and determines whether access should be granted or redirected.
        /// </summary>
        /// <param name="context">The filter context encapsulating the current HTTP request information.</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var role = context.HttpContext.Session.GetString("Role")?.ToLower(); // Retrieves the current user role from session storage and normalizes it for matching

            // Validates that an active session role exists and matches at least one permitted security tier
            if (role == null || !_roles.Contains(role))
            {
                // Redirects unauthenticated or unauthorized users to the login endpoint to safeguard protected views
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
        }
    }
}