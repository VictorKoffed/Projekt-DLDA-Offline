using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs.Question;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Manages administrative question catalog workflows, including creation, modification, 
/// and archival operations, secured exclusively for administrator security roles.
/// </summary>
[RoleAuthorize("admin")]
public class AdminQuestionController : Controller
{
    private readonly QuestionAdminService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminQuestionController"/> class.
    /// </summary>
    /// <param name="service">The administrative service handling question repository operations.</param>
    public AdminQuestionController(QuestionAdminService service)
    {
        _service = service;
    }

    /// <summary>
    /// Renders an index listing of all master questionnaire catalog items.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var questions = await _service.GetAllQuestionsAsync();
        return View("Index", questions);
    }

    /// <summary>
    /// Renders the creation form view for defining a new question template.
    /// </summary>
    public IActionResult Create() => View("Create", new Question());

    /// <summary>
    /// Processes submission data to create and persist a new question definition in the catalog.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(Question dto)
    {
        if (!ModelState.IsValid) return View("Create", dto);

        var success = await _service.CreateQuestionAsync(dto);
        TempData[success ? "Success" : "Error"] = success
            ? "Frågan skapades."
            : "Kunde inte skapa frågan.";
        return success ? RedirectToAction("Index") : View("Create", dto);
    }

    /// <summary>
    /// Renders the modification form view populated with existing properties for a specific target question.
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        var question = await _service.GetQuestionByIdAsync(id);
        if (question == null)
        {
            TempData["Error"] = "Kunde inte hitta frågan.";
            return RedirectToAction("Index");
        }

        return View("Edit", question);
    }

    /// <summary>
    /// Validates route integrity against payload identifiers and commits updates to an existing question definition.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Question dto)
    {
        if (id != dto.QuestionID) return BadRequest();
        if (!ModelState.IsValid) return View("Edit", dto);

        var success = await _service.UpdateQuestionAsync(id, dto);
        TempData[success ? "Success" : "Error"] = success
            ? "Frågan uppdaterades."
            : "Kunde inte uppdatera frågan.";
        return success ? RedirectToAction("Index") : View("Edit", dto);
    }

    /// <summary>
    /// Renders the confirmation view prior to permanently removing or deactivating a question item.
    /// </summary>
    public async Task<IActionResult> Delete(int id)
    {
        var question = await _service.GetQuestionByIdAsync(id);
        if (question == null)
        {
            TempData["Error"] = "Kunde inte hitta frågan.";
            return RedirectToAction("Index");
        }

        return View("Delete", question);
    }

    /// <summary>
    /// Executes the removal or deactivation of the specified question definition upon administrative confirmation.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var success = await _service.DeleteQuestionAsync(id);
        TempData[success ? "Success" : "Error"] = success
            ? "Frågan togs bort."
            : "Kunde inte ta bort frågan.";
        return RedirectToAction("Index");
    }
}