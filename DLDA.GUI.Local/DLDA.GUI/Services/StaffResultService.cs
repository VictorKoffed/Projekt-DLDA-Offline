using DLDA.GUI.DTOs.Staff;
using System.Net.Http.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Service class responsible for managing the summary and finalization of a staff member's assessment.
    /// Acts as a clear boundary layer, keeping the MVC presentation logic decoupled from the API's assessment finalization and state-management rules.
    /// </summary>
    public class StaffResultService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StaffResultService> _logger;

        /// <summary>
        /// Initializes a new instance of the StaffResultService.
        /// </summary>
        /// <param name="factory">Provides a pre-configured HttpClient. Utilizing IHttpClientFactory prevents socket exhaustion and manages DNS lifecycle automatically.</param>
        /// <param name="logger">Records network or parsing failures to support operational observability and debugging.</param>
        public StaffResultService(IHttpClientFactory factory, ILogger<StaffResultService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the comprehensive overview of the staff's answers and comments.
        /// Used at the end of the quiz flow to present a summary view, allowing the healthcare professional to verify their inputs before final submission.
        /// </summary>
        /// <param name="assessmentId">The unique identifier of the assessment.</param>
        /// <returns>A DTO containing the overview data, or null if the API call fails. Returning null allows the UI to degrade gracefully instead of crashing.</returns>
        public async Task<StaffResultOverviewDto?> GetOverviewAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"AssessmentItem/staff/assessment/{assessmentId}/overview");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<StaffResultOverviewDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetOverviewAsync({AssessmentId})", assessmentId);
                return null;
            }
        }

        /// <summary>
        /// Updates a previously submitted staff answer for a specific question.
        /// Essential for the summary phase, as it allows healthcare professionals to revise their evaluations directly from the overview screen without restarting the workflow.
        /// </summary>
        /// <param name="dto">The payload containing the updated answer data.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        public async Task<bool> UpdateStaffAnswerAsync(SubmitStaffAnswerDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("Question/quiz/staff/submit", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid UpdateStaffAnswerAsync för ItemID={ItemID}", dto.ItemID);
                return false;
            }
        }

        /// <summary>
        /// Marks an assessment as fully completed from the staff's perspective.
        /// </summary>
        /// <param name="assessmentId">The unique identifier of the assessment.</param>
        /// <returns>True if the completion was successful, otherwise false.</returns>
        public async Task<bool> CompleteStaffAssessmentAsync(int assessmentId)
        {
            try
            {
                // We issue a POST request with a null payload because the URL itself explicitly defines the state transition ("staff-complete").
                // This correctly delegates the state mutation logic to the backend business rules.
                var response = await _httpClient.PostAsync($"AssessmentItem/assessment/{assessmentId}/staff-complete", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid CompleteStaffAssessmentAsync({AssessmentId})", assessmentId);
                return false;
            }
        }

        /// <summary>
        /// Unlocks a previously completed assessment.
        /// Acts as a fail-safe mechanism, allowing healthcare professionals to reopen and correct an assessment that was finalized by mistake.
        /// </summary>
        /// <param name="assessmentId">The unique identifier of the assessment to unlock.</param>
        /// <returns>True if the unlock operation was successful, otherwise false.</returns>
        public async Task<bool> UnlockAssessmentAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"assessment/unlock/{assessmentId}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid UnlockAssessmentAsync({AssessmentId})", assessmentId);
                return false;
            }
        }
    }
}