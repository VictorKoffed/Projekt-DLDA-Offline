using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using DLDA.GUI.DTOs.Assessment;
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.Controllers
{
    /// <summary>
    /// Manages healthcare professional workflows for overseeing patient assessments, 
    /// managing session lifecycles, and filtering clinical dashboards securely.
    /// </summary>
    [RoleAuthorize("staff")]
    public class StaffAssessmentController : Controller
    {
        private readonly StaffAssessmentService _service;

        public StaffAssessmentController(StaffAssessmentService service)
        {
            _service = service;
        }

        /// <summary>
        /// Renders a filtered list view of patients alongside their latest assessment statuses.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? search, bool? ongoing, bool? notOngoing, string? recent)
        {
            var patients = await _service.GetFilteredPatientsAsync(search, ongoing, notOngoing, recent);
            return View(patients);
        }


        /// <summary>
        /// Executes patient catalog searches based on specified text queries.
        /// </summary>
        public async Task<IActionResult> Patients(string? search)
        {
            var patients = await _service.SearchPatientsAsync(search);
            return View(patients);
        }

        /// <summary>
        /// Renders all historical and active assessment sessions belonging to a specific patient.
        /// </summary>
        public async Task<IActionResult> Assessments(int userId)
        {
            ViewBag.UserId = userId;
            ViewBag.Username = await _service.GetUsernameAsync(userId);

            var assessments = await _service.GetAssessmentsForUserAsync(userId);
            return View("Assessments", assessments);
        }

        /// <summary>
        /// Provisions a new assessment container session for the specified patient profile.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateForPatient(int userId)
        {
            var success = await _service.CreateAssessmentAsync(userId);

            TempData[success ? "Success" : "Error"] = success
                ? "Ny bedömning skapades."
                : "Misslyckades att skapa bedömning.";

            return RedirectToAction("Assessments", new { userId });
        }

        /// <summary>
        /// Renders the confirmation view prior to permanently deleting an assessment record.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var assessment = await _service.GetAssessmentAsync(id);
            if (assessment == null) return View("Error");

            return View("Delete", assessment);
        }

        /// <summary>
        /// Executes the permanent removal of the specified assessment session upon confirmation.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int userId)
        {
            var success = await _service.DeleteAssessmentAsync(id);

            TempData[success ? "Success" : "Error"] = success
                ? "Bedömning togs bort."
                : "Misslyckades att ta bort bedömning.";

            return RedirectToAction("Assessments", new { userId });
        }
    }
}