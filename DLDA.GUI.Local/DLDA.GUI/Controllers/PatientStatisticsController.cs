using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs.Patient;
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Manages patient-facing statistical analytics, historical trend tracking, 
/// and assessment comparison dashboards, secured exclusively for patient security roles.
/// </summary>
[Route("PatientStatistics")]
[RoleAuthorize("patient")]
public class PatientStatisticsController : Controller
{
    private readonly PatientStatisticsService _service;
    private readonly ILogger<PatientStatisticsController> _logger;

    public PatientStatisticsController(PatientStatisticsService service, ILogger<PatientStatisticsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Renders detailed granular statistics and raw question-by-question responses for a specific assessment session.
    /// </summary>
    [HttpGet("Single/{assessmentId}")]
    public async Task<IActionResult> Single(int assessmentId)
    {
        var answers = await _service.GetAnswersForAssessmentAsync(assessmentId);
        if (answers.Count == 0)
        {
            TempData["Error"] = "Statistiken kan inte visas eftersom denna bedömning saknar besvarade frågor.";
            return RedirectToAction("Index", "PatientAssessment");
        }

        var assessment = await _service.GetAssessmentAsync(assessmentId);
        if (assessment == null)
        {
            TempData["Error"] = "Bedömningen kunde inte hittas.";
            return RedirectToAction("Index", "PatientAssessment");
        }

        var model = new PatientStatistics
        {
            AssessmentId = assessmentId,
            CreatedAt = assessment.CreatedAt,
            Answers = answers
        };

        return View("Single", model);
    }

    /// <summary>
    /// Renders an aggregated summary view of statistical metrics for an assessment.
    /// </summary>
    [HttpGet("Overview")]
    [HttpGet("Overview/{assessmentId}")]
    public async Task<IActionResult> Overview(int assessmentId)
    {
        var summary = await _service.GetSummaryAsync(assessmentId);
        if (summary == null)
        {
            TempData["Error"] = "Statistiken kunde inte visas eftersom denna bedömning saknar besvarade frågor.";
            return RedirectToAction("Index", "PatientAssessment");
        }

        return View("Single", summary);
    }

    /// <summary>
    /// Renders longitudinal improvement trends over time, requiring a minimum of two completed assessment sessions.
    /// </summary>
    [HttpGet("Improvement/{userId}")]
    public async Task<IActionResult> Improvement(int userId)
    {
        // Retrieves aggregated longitudinal trend metrics from the service layer
        var data = await _service.GetImprovementDataAsync(userId);

        // Validates that sufficient historical data exists to plot progress over time
        if (data == null)
        {
            _logger.LogWarning("Förbättringsdatan är null för användaren {UserId}. Kontrollera att två avslutade bedömningar finns.", userId);
            TempData["Error"] = "För att kunna visa förbättringar över tid behöver du ha gjort minst två bedömningar.";
            return RedirectToAction("Index", "PatientAssessment");
        }

        // Renders the trend visualization view once baseline prerequisites are confirmed
        return View("Improvement", data);
    }

    /// <summary>
    /// Compares metrics between two distinct assessment sessions to highlight delta changes.
    /// </summary>
    [HttpPost("Compare")]
    public async Task<IActionResult> Compare(int firstId, int secondId)
    {
        // Prevents redundant comparison operations against identical assessment identifiers
        if (firstId == secondId)
        {
            TempData["Error"] = "Du måste välja två olika bedömningar att jämföra.";
            return RedirectToAction("Index", "PatientAssessment");
        }

        var data = await _service.CompareAssessmentsAsync(firstId, secondId);
        if (data == null)
        {
            TempData["Error"] = "Jämförelsen kunde inte göras. Kontrollera att båda bedömningarna har besvarade frågor.";
            return RedirectToAction("Index", "PatientAssessment");
        }

        return View("Improvement", data); // Reuses the existing trend visualization layout view for comparative analysis
    }

}