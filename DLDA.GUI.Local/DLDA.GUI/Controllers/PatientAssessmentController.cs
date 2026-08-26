using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.Controllers
{
    /// <summary>
    /// Manages patient-facing assessment overview dashboards, secured exclusively 
    /// for users holding the patient security role clearance.
    /// </summary>
    [RoleAuthorize("patient")]
    public class PatientAssessmentController : Controller
    {
        private readonly PatientAssessmentService _service;
        private readonly ILogger<PatientAssessmentController> _logger;

        public PatientAssessmentController(PatientAssessmentService service, ILogger<PatientAssessmentController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Renders an index listing of all historical and active assessments belonging to the authenticated patient session.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            // Validates active session context presence to prevent unauthorized unlinked requests
            if (userId == null)
            {
                _logger.LogWarning("Ingen inloggad användare – redirect till login.");
                return RedirectToAction("Login", "Account");
            }

            var assessments = await _service.GetAssessmentsForUserAsync(userId.Value);
            return View(assessments);
        }
    }
}