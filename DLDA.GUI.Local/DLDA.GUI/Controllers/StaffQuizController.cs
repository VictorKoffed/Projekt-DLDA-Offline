using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs.Staff;
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.Controllers
{
    /// <summary>
    /// Manages the healthcare professional interactive review wizard workflow, 
    /// enabling clinical evaluation scoring, risk flagging, commentary inputs, and questionnaire navigation.
    /// </summary>
    [RoleAuthorize("staff")]
    public class StaffQuizController : Controller
    {
        private readonly StaffQuizService _service;

        public StaffQuizController(StaffQuizService service)
        {
            _service = service;
        }

        /// <summary>
        /// Resumes the professional evaluation wizard by fetching the next pending item or routing 
        /// to the final results summary if all questions have been reviewed.
        /// </summary>
        [HttpGet("StaffQuiz/Resume")]
        public async Task<IActionResult> Resume(int assessmentId, int userId)
        {
            var question = await _service.GetNextQuestionAsync(assessmentId);

            if (question == null)
            {
                TempData["Success"] = "Du har gått igenom alla frågor.";
                return RedirectToAction("Index", "StaffResult", new { id = assessmentId });
            }

            var totalQuestions = await _service.GetTotalQuestionCountForStaffAsync(assessmentId);

            ViewBag.AssessmentId = assessmentId;
            ViewBag.UserId = userId;
            ViewBag.TotalQuestions = totalQuestions ?? 0;

            return View("Question", question);
        }

        /// <summary>
        /// Submits the professional evaluation rating score, clinical commentary, and risk flag 
        /// for a specific assessment line item, then advances the review wizard.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(int itemId, int assessmentId, int answer, string? comment, bool flag, int userId)
        {
            var dto = new SubmitStaffAnswerDto
            {
                ItemID = itemId,
                Answer = answer,
                Comment = comment,
                Flag = flag
            };

            var success = await _service.SubmitAnswerAsync(dto);
            if (!success)
                TempData["Error"] = "Kunde inte spara svaret.";

            return RedirectToAction("Resume", new { assessmentId, userId });
        }

        /// <summary>
        /// Retrieves and renders the preceding question within the professional review sequence 
        /// to allow clinicians to inspect or modify prior evaluations.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Previous(int assessmentId, int currentOrder, int userId)
        {
            var question = await _service.GetPreviousQuestionAsync(assessmentId, currentOrder);

            if (question == null)
            {
                TempData["Error"] = "Kunde inte hämta föregående fråga.";
                return RedirectToAction("Resume", new { assessmentId, userId });
            }

            var totalQuestions = await _service.GetTotalQuestionCountForStaffAsync(assessmentId);

            ViewBag.AssessmentId = assessmentId;
            ViewBag.UserId = userId;
            ViewBag.TotalQuestions = totalQuestions ?? 0;

            return View("Question", question);
        }


        /// <summary>
        /// Temporarily halts the professional review session and returns the clinician 
        /// to the patient assessment list dashboard.
        /// </summary>
        [HttpPost("StaffQuiz/Pause")]
        public IActionResult Pause(int assessmentId, int userId)
        {
            TempData["Info"] = "Bedömningen är pausad. Du kan återuppta den senare.";
            return RedirectToAction("Assessments", "StaffAssessment", new { userId });
        }

        /// <summary>
        /// Bypasses the current questionnaire item while preserving any associated clinical commentary or risk flags.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SkipQuestion(int itemId, int assessmentId, string? comment, bool flag, int userId)
        {
            var success = await _service.SkipQuestionAsync(itemId, comment, flag);

            if (!success)
                TempData["Error"] = "Kunde inte hoppa över frågan.";

            return RedirectToAction("Resume", new { assessmentId, userId });
        }
    }
}