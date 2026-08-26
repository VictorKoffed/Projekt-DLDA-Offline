using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using DLDA.GUI.DTOs.Staff;
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Manages healthcare professional statistical analytics views, comparative matrices, 
/// longitudinal progress tracking, and response distribution breakdowns, secured exclusively for staff security roles.
/// </summary>
[Route("StaffStatistics")]
[RoleAuthorize("staff")]
public class StaffStatisticsController : Controller
{
    private readonly StaffStatisticsService _service;

    public StaffStatisticsController(StaffStatisticsService service)
    {
        _service = service;
    }

    [HttpGet("Comparison/{assessmentId}")]
    public async Task<IActionResult> Comparison(int assessmentId)
    {
        try
        {
            var result = await _service.GetComparisonAsync(assessmentId);
            var comparison = result.Comparison;
            var assessment = result.Assessment;

            if (assessment == null)
            {
                TempData["Error"] = "Bedömningen kunde inte hittas.";
                return RedirectToAction("Index", "StaffAssessment");
            }

            int userId = assessment.UserId; // Extracts and securely preserves the patient identifier context for fallback redirects

            if (comparison == null || !comparison.Any())
            {
                TempData["Error"] = "Jämförelsen kan inte visas eftersom patienten eller personalen inte har svarat på några frågor i denna bedömning.";
                return RedirectToAction("Assessments", "StaffAssessment", new { userId });
            }

            ViewBag.UserId = userId;
            ViewBag.AssessmentId = assessment.AssessmentID;
            ViewBag.PatientName = comparison.First().Username;
            ViewBag.AssessmentDate = comparison.First().CreatedAt;

            return View("Comparison", comparison);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ett tekniskt fel uppstod: {ex.Message}";
            return RedirectToAction("Assessments", "StaffAssessment");
        }
    }



    /// <summary>
    /// Renders longitudinal improvement or regression trajectories across multiple patient assessments from a staff perspective.
    /// </summary>
    [HttpGet("ChangeOverview/{userId}")]
    public async Task<IActionResult> ChangeOverview(int userId)
    {
        try
        {
            var overview = await _service.GetChangeOverviewAsync(userId);

            if (overview == null)
            {
                TempData["Error"] = "Det finns inte tillräckligt med svar i bedömningarna för att visa en jämförelse över tid.";
                return RedirectToAction("Assessments", "StaffAssessment", new { userId });
            }

            ViewBag.UserId = userId;
            return View("ChangeOverview", overview);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ett tekniskt fel uppstod: {ex.Message}";
            return RedirectToAction("Assessments", "StaffAssessment", new { userId });
        }
    }

    /// <summary>
    /// Renders the patient's score distribution metrics formatted for graphical chart visualization.
    /// </summary>
    [HttpGet("PatientAnswerSummary/{assessmentId}")]
    public async Task<IActionResult> PatientAnswerSummary(int assessmentId)
    {
        try
        {
            var result = await _service.GetComparisonAsync(assessmentId);
            var data = result.Comparison;
            var assessment = result.Assessment;

            if (data == null || !data.Any() || assessment == null)
            {
                TempData["Error"] = "Kunde inte hämta patientens svar.";
                return RedirectToAction("Comparison", new { assessmentId });
            }

            var first = data.First();

            ViewBag.PatientName = first.Username;
            ViewBag.AssessmentDate = first.CreatedAt;
            ViewBag.UserId = assessment.UserId;
            ViewBag.AssessmentId = assessment.AssessmentID;

            return View("PatientAnswerSummary", data);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ett tekniskt fel uppstod: {ex.Message}";
            return RedirectToAction("Comparison", new { assessmentId });
        }
    }

    /// <summary>
    /// Compares two completed professional assessment sessions to evaluate clinical evaluation shifts over time.
    /// </summary>
    [HttpPost("Compare")]
    public async Task<IActionResult> Compare(int userId, int firstId, int secondId)
    {
        if (firstId == secondId)
        {
            TempData["Error"] = "Du måste välja två olika bedömningar att jämföra.";
            return RedirectToAction("Assessments", "StaffAssessment", new { userId });
        }

        var result = await _service.CompareAssessmentsAsync(firstId, secondId);
        if (result == null)
        {
            TempData["Error"] = "Jämförelsen kunde inte göras. Kontrollera att båda bedömningarna har tillräckligt med svar.";
            return RedirectToAction("Assessments", "StaffAssessment", new { userId });
        }

        ViewBag.UserId = userId;
        ViewBag.FirstId = firstId;   // Preserves session identifiers in ViewBag state to maintain comparison context across view redirects
        ViewBag.SecondId = secondId;

        return View("ChangeOverview", result);
    }


    /// <summary>
    /// Compares two historical patient self-assessments to visualize patient behavioral or symptomatic shifts over time for clinicians.
    /// </summary>
    public async Task<IActionResult> ComparePatientAnswersForStaff(int userId, int firstId, int secondId)
    {
        if (firstId == secondId)
        {
            TempData["Error"] = "Du måste välja två olika bedömningar att jämföra.";
            return RedirectToAction("Assessments", "StaffAssessment", new { userId });
        }

        var result = await _service.ComparePatientAnswersForStaffAsync(firstId, secondId);
        if (result == null)
        {
            TempData["Error"] = "Kunde inte hämta förändringar i patientens svar över tid.";
            return RedirectToAction("Assessments", "StaffAssessment", new { userId });
        }

        ViewBag.UserId = userId;
        ViewBag.FirstId = firstId;
        ViewBag.SecondId = secondId;

        return View("PatientChangeOverviewForStaff", result);
    }
}