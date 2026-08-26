using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs.Patient;  
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.Controllers
{
    /// <summary>
    /// Manages the patient questionnaire interactive wizard workflow, guiding users 
    /// through assessment scale selection, item responses, skipping logic, and navigation.
    /// </summary>
    [RoleAuthorize("patient")]
    public class PatientQuizController : Controller
    {
        private readonly PatientQuizService _service;

        public PatientQuizController(PatientQuizService service)
        {
            _service = service;
        }

        /// <summary>
        /// Renders the informational pre-assessment entry view, displaying completion state 
        /// and progress indicators prior to initiating or resuming the wizard.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Info(int id)
        {
            var assessment = await _service.GetAssessmentAsync(id);
            if (assessment == null)
            {
                TempData["Error"] = "Kunde inte hämta information om bedömningen.";
                return View("Error");
            }

            ViewBag.AssessmentId = id;
            ViewBag.HasStarted = assessment.HasStarted;
            return View();
        }

        /// <summary>
        /// Renders the configuration view allowing the patient to choose their scoring scale type for the session.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ScaleSelect(int id)
        {
            var assessment = await _service.GetAssessmentAsync(id);
            if (assessment == null) return View("Error");
            return View(assessment);
        }

        /// <summary>
        /// Persists the selected evaluation scale choice and advances the user directly into the active quiz flow.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SetScale(int id, string selectedScale)
        {
            var success = await _service.UpdateScaleAsync(id, selectedScale);
            if (!success)
            {
                TempData["Error"] = "Kunde inte spara vald skala.";
                return RedirectToAction("ScaleSelect", new { id });
            }

            return RedirectToAction("Resume", new { id });
        }

        /// <summary>
        /// Resumes the assessment wizard by fetching the next unanswered question or routing 
        /// to the result view if all items have been completed.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Resume(int id)
        {
            var assessment = await _service.GetAssessmentAsync(id);
            if (assessment == null) return View("Error");

            if (assessment.IsComplete)
                return RedirectToAction("Index", "PatientResult", new { assessmentId = id });

            var question = await _service.GetNextQuestionAsync(id);
            if (question == null)
            {
                TempData["Error"] = "Du har svarat på alla frågor.";
                return RedirectToAction("Index", "PatientResult", new { assessmentId = id });
            }

            var totalQuestions = await _service.GetTotalQuestionCountAsync(id);
            ViewBag.TotalQuestions = totalQuestions ?? 0; // Fallback: defaults to 0 if count retrieval fails to prevent view rendering crashes
            ViewBag.AssessmentId = id;

            return View("Question", question);
        }

        /// <summary>
        /// Submits a patient's numerical rating and optional commentary for the targeted assessment item, 
        /// then automatically advances to the subsequent question.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(int itemId, int assessmentId, int answer, string? comment)
        {
            var dto = new PatientAnswerDto
            {
                Answer = answer,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment
            };

            var success = await _service.SubmitAnswerAsync(itemId, dto);
            if (!success)
            {
                TempData["Error"] = "Kunde inte spara svaret.";
            }

            return RedirectToAction("Resume", new { id = assessmentId });
        }

        /// <summary>
        /// Flags a specific question item as intentionally bypassed by the patient and advances the wizard.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SkipQuestion(int itemId, int assessmentId)
        {
            var success = await _service.SkipQuestionAsync(itemId);
            if (!success)
            {
                TempData["Error"] = "Kunde inte hoppa över frågan.";
            }

            return RedirectToAction("Resume", new { id = assessmentId });
        }

        /// <summary>
        /// Retrieves and renders the preceding question based on sequence order index for review or modification.
        /// </summary>
        public async Task<IActionResult> Previous(int assessmentId, int currentOrder)
        {
            var question = await _service.GetPreviousQuestionAsync(assessmentId, currentOrder);
            if (question == null)
            {
                TempData["Error"] = "Kunde inte hämta föregående fråga.";
                return RedirectToAction("Resume", new { id = assessmentId });
            }

            var totalQuestions = await _service.GetTotalQuestionCountAsync(assessmentId);
            ViewBag.TotalQuestions = totalQuestions ?? 0; // Fallback: handles missing aggregate counts safely
            ViewBag.AssessmentId = assessmentId;

            return View("Question", question);
        }

        /// <summary>
        /// Temporarily halts the active quiz session, allowing the user to resume their progress later from the dashboard.
        /// </summary>
        [HttpPost]
        public IActionResult Pause(int assessmentId)
        {
            TempData["Success"] = "Du kan återuppta din bedömning senare.";
            return RedirectToAction("Index", "PatientAssessment");
        }
    }
}