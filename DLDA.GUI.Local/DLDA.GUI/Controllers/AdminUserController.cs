using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs.User;
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Manages administrative user account lifecycle workflows (CRUD operations), 
/// securing access exclusively for users holding administrator security clearance.
/// </summary>
[RoleAuthorize("admin")]
public class AdminUserController : Controller
{
    private readonly UserAdminService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUserController"/> class.
    /// </summary>
    /// <param name="service">The administrative service handling user repository operations.</param>
    public AdminUserController(UserAdminService service)
    {
        _service = service;
    }

    /// <summary>
    /// Renders an index listing of all registered system user accounts.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var users = await _service.GetAllAsync();
        return View(users);
    }

    /// <summary>
    /// Renders the form view for provisioning a new user account profile.
    /// </summary>
    public IActionResult Create() => View(new UserDto());

    /// <summary>
    /// Processes submission data to create and register a new user account within the system repository.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(UserDto user)
    {
        if (!ModelState.IsValid) return View(user);

        var success = await _service.CreateAsync(user);
        TempData[success ? "Success" : "Error"] = success
            ? "Användaren skapades."
            : "Det gick inte att skapa användaren.";
        return success ? RedirectToAction("Index") : View(user);
    }

    /// <summary>
    /// Renders the modification form view populated with existing profile data for a specific target user.
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _service.GetByIdAsync(id);
        if (user == null)
        {
            TempData["Error"] = "Kunde inte hämta användaren.";
            return RedirectToAction("Index");
        }

        return View(user);
    }

    /// <summary>
    /// Validates route identity parameters against payload identifiers and commits profile updates for the user.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Edit(int id, UserDto user)
    {
        if (id != user.UserID) return BadRequest();
        if (!ModelState.IsValid) return View(user);

        var success = await _service.UpdateAsync(id, user);
        TempData[success ? "Success" : "Error"] = success
            ? "Användaren uppdaterades."
            : "Kunde inte uppdatera användaren.";
        return success ? RedirectToAction("Index") : View(user);
    }

    /// <summary>
    /// Renders the confirmation view prior to permanently removing a user account.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _service.GetByIdAsync(id);
        if (user == null)
        {
            TempData["Error"] = "Kunde inte hämta användaren.";
            return RedirectToAction("Index");
        }

        return View(user);
    }

    /// <summary>
    /// Executes the permanent removal of the specified user account upon administrative confirmation.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(int userID)
    {
        var success = await _service.DeleteAsync(userID);
        TempData[success ? "Success" : "Error"] = success
            ? "Användaren togs bort."
            : "Kunde inte ta bort användaren.";
        return RedirectToAction("Index");
    }
}