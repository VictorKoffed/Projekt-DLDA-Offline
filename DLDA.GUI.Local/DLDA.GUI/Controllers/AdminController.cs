using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DLDA.GUI.Authorization;

namespace DLDA.GUI.Controllers
{
    /// <summary>
    /// Manages administrative panel workflows and views, secured via role-based access control 
    /// restricted exclusively to users holding the administrator security clearance.
    /// </summary>
    [RoleAuthorize("admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance used for auditing administrative actions and events.</param>
        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Renders the primary administrative dashboard view and logs entry audit details.
        /// </summary>
        /// <returns>The admin panel Razor view.</returns>
        public IActionResult Index()
        {
            // Audits administrative dashboard access for security tracing and usage monitoring
            _logger.LogInformation("Adminpanelen öppnades av användare {User}", HttpContext.Session.GetString("Username"));
            return View();
        }
    }
}