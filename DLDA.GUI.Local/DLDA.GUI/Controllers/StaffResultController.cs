using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs.Staff;
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.Controllers
{
    /// <summary>
    /// Manages healthcare professional result review dashboards, comparative scoring matrices, 
    /// clinical sign-off completions, and session unlocks, secured exclusively for staff security roles.
    /// </summary>
    [RoleAuthorize("staff")]
    public class StaffResultController : Controller
    {
        private readonly StaffResultService _service;

        public StaffResultController(StaffResultService service)
        {
            _service = service;
        }

        /// <summary>
        /// Renders the comprehensive staff results matrix comparing patient self-assessments 
        /// against clinical professional evaluations.
        /// </summary>
        public async Task<IActionResult> Index(int id)
        {
            var overview = await _service.GetOverviewAsync(id);

            if (overview == null)
            {
                TempData["Error"] = "Kunde inte hämta personalsammanställning.";
                return RedirectToAction("Index", "StaffAssessment");
            }

            return View("Index", overview);
        }

        /// <summary>
        /// Processes inline modifications to professional scores, clinical commentary, 
        /// or risk flags directly from the results matrix view.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateStaffAnswer(int itemId, int assessmentId, int answer, string? comment, bool flag)
        {
            var dto = new SubmitStaffAnswerDto
            {
                ItemID = itemId,
                Answer = answer,
                Comment = comment,
                Flag = flag
            };

            var success = await _service.UpdateStaffAnswerAsync(dto);
            TempData[success ? "Success" : "Error"] = success
                ? "Svar uppdaterat."
                : "Kunde inte spara ändringar.";

            return RedirectToAction("Index", new { id = assessmentId });
        }

        /// <summary>
        /// Finalizes the healthcare professional review by committing a completion sign-off, 
        /// locking the evaluation against further accidental modifications.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Complete(int assessmentId, int userId)
        {
            var success = await _service.CompleteStaffAssessmentAsync(assessmentId);
            TempData[success ? "Success" : "Error"] = success
                ? "Personalens bedömning har markerats som klar."
                : "Kunde inte markera bedömningen som klar. Kontrollera att alla frågor är besvarade.";

            return success
                ? RedirectToAction("Assessments", "StaffAssessment", new { userId })
                : RedirectToAction("Index", new { id = assessmentId });
        }

        /// <summary>
        /// Revokes a prior clinical completion sign-off to reopen a locked assessment session 
        /// for necessary adjustments or follow-up reviews.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Unlock(int assessmentId, int userId)
        {
            var success = await _service.UnlockAssessmentAsync(assessmentId);
            TempData[success ? "Success" : "Error"] = success
                ? "Bedömningen har låsts upp."
                : "Misslyckades med att låsa upp bedömningen.";

            return success
                ? RedirectToAction("Index", new { id = assessmentId })
                : RedirectToAction("Assessments", "StaffAssessment", new { userId });
        }
    }
}