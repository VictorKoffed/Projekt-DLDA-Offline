using DLDA.GUI.DTOs.Assessment;
using DLDA.GUI.DTOs.Patient;
using System.Net.Http.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Service class responsible for managing the result overview and completion of a patient's assessment.
    /// Acts as a dedicated boundary between the MVC presentation layer and the API for the final stages 
    /// of the assessment workflow, ensuring that finalization logic remains decoupled from the UI.
    /// </summary>
    public class PatientResultService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PatientResultService> _logger;

        /// <summary>
        /// Initializes a new instance of the PatientResultService.
        /// </summary>
        /// <param name="factory">Provides a pre-configured HttpClient. Relying on IHttpClientFactory mitigates socket exhaustion and handles DNS resolution updates automatically.</param>
        /// <param name="logger">Captures network or deserialization failures to support operational observability.</param>
        public PatientResultService(IHttpClientFactory factory, ILogger<PatientResultService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Retrieves metadata for a specific assessment.
        /// </summary>
        /// <param name="assessmentId">The unique identifier of the assessment.</param>
        /// <returns>The assessment metadata, or null if the request fails. Returning null instead of throwing an exception allows the caller to handle missing data gracefully (e.g., displaying a user-friendly error or redirecting).</returns>
        public async Task<AssessmentDto?> GetAssessmentAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Assessment/{assessmentId}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<AssessmentDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetAssessmentAsync({AssessmentId})", assessmentId);
                return null;
            }
        }

        /// <summary>
        /// Retrieves the comprehensive overview of answers and comments for a given assessment.
        /// Used primarily at the end of the quiz flow to present a summary view to the patient before final submission.
        /// </summary>
        /// <param name="assessmentId">The unique identifier of the assessment.</param>
        /// <returns>A DTO containing the overview data, or null if the API call fails.</returns>
        public async Task<AssessmentOverviewDto?> GetOverviewAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"AssessmentItem/patient/assessment/{assessmentId}/overview");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<AssessmentOverviewDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetOverviewAsync({AssessmentId})", assessmentId);
                return null;
            }
        }

        /// <summary>
        /// Marks an assessment as completed.
        /// </summary>
        /// <param name="assessmentId">The unique identifier of the assessment to complete.</param>
        /// <returns>True if the completion was successful, otherwise false.</returns>
        public async Task<bool> CompleteAssessmentAsync(int assessmentId)
        {
            try
            {
                // We issue a POST request with a null payload because the URL itself explicitly defines the state transition ("complete").
                // This delegates the state mutation logic entirely to the backend business rules.
                var response = await _httpClient.PostAsync($"AssessmentItem/assessment/{assessmentId}/complete", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid CompleteAssessmentAsync({AssessmentId})", assessmentId);
                return false;
            }
        }

        /// <summary>
        /// Updates a previously submitted answer for an assessment item.
        /// </summary>
        /// <param name="itemId">The unique ID of the specific assessment item.</param>
        /// <param name="dto">The payload containing the updated answer data.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        public async Task<bool> UpdateAnswerAsync(int itemId, PatientAnswerDto dto)
        {
            try
            {
                // We use a PUT request here because we are idempotently mutating an existing assessment item record 
                // in the database, allowing users to revise their answers in the overview step before final completion.
                var response = await _httpClient.PutAsJsonAsync($"AssessmentItem/patient/{itemId}", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid UpdateAnswerAsync({ItemId})", itemId);
                return false;
            }
        }
    }
}