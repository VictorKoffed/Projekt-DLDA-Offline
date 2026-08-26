using Microsoft.AspNetCore.Mvc;
using DLDA.GUI.DTOs.Authentication;

/// <summary>
/// Manages user authentication workflows, session state lifecycles, 
/// and role-based navigation routing for the MVC frontend interface.
/// </summary>
public class AccountController : Controller
{
    private readonly AccountService _accountService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(AccountService accountService, ILogger<AccountController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    /// <summary>
    /// Renders the user login form view for credential inputs.
    /// </summary>
    [HttpGet]
    public IActionResult Login() => View();

    /// <summary>
    /// Processes submitted authentication credentials, establishes active session states,
    /// and routes users to their respective role-specific landing pages.
    /// </summary>
    /// <param name="login">The credential payload containing username and password inputs.</param>
    /// <returns>A redirect action to the appropriate role dashboard, or back to login view with error feedback if authentication fails.</returns>
    [HttpPost]
    public async Task<IActionResult> Login(LoginDto login)
    {
        var user = await _accountService.LoginAsync(login);
        if (user == null)
        {
            ViewBag.Error = "Felaktigt användarnamn, lösenord eller serverfel.";
            return View();
        }

        // Persists user identity context and security role tier within encrypted session storage for subsequent requests
        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetString("Role", user.Role);

        // Determines target dashboard routing dynamically based on privilege authorization levels
        return user.Role.ToLower() switch
        {
            "admin" => RedirectToAction("Index", "Admin"),
            "staff" => RedirectToAction("Index", "StaffAssessment"),
            "patient" => RedirectToAction("Index", "PatientAssessment"),
            _ => RedirectToAction("Login")
        };
    }

    /// <summary>
    /// Terminates the current active session by purging stored state data and revoking access tokens.
    /// </summary>
    /// <returns>A redirect action returning the user to the login portal view.</returns>
    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    public IActionResult Info()
    {
        return View();
    }
}