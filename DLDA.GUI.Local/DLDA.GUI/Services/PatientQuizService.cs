using DLDA.GUI.DTOs.Assessment;
using DLDA.GUI.DTOs.Patient;
using DLDA.GUI.DTOs.Question;
using System.Net.Http.Json;
using System.Text.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Service class responsible for managing the patient's assessment (quiz) workflow.
    /// It acts as a stateless orchestrator, relying on the API to dictate next/previous steps, 
    /// ensuring the GUI remains decoupled from the underlying business rules of the DLDA assessment.
    /// </summary>
    public class PatientQuizService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PatientQuizService> _logger;

        /// <summary>
        /// Initializes a new instance of the PatientQuizService.
        /// </summary>
        /// <param name="factory">Provides a pre-configured HttpClient. Using IHttpClientFactory prevents socket exhaustion and manages the DNS lifecycle automatically.</param>
        /// <param name="logger">Records API integration failures for observability and troubleshooting.</param>
        public PatientQuizService(IHttpClientFactory factory, ILogger<PatientQuizService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the base assessment metadata by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the assessment.</param>
        /// <returns>The assessment details, or null if the API call fails, allowing the UI to handle the error gracefully.</returns>
        public async Task<AssessmentDto?> GetAssessmentAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Assessment/{id}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<AssessmentDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetAssessmentAsync({Id})", id);
                return null;
            }
        }

        /// <summary>
        /// Updates the evaluation scale type for a specific assessment.
        /// </summary>
        /// <param name="id">The assessment ID.</param>
        /// <param name="scale">The chosen scale identifier.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        public async Task<bool> UpdateScaleAsync(int id, string scale)
        {
            // We use a read-modify-write pattern here. 
            // Fetching the existing DTO first ensures we don't inadvertently nullify or overwrite 
            // other properties of the assessment when issuing the PUT request.
            var dto = await GetAssessmentAsync(id);
            if (dto == null) return false;

            dto.ScaleType = scale;

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"Assessment/{id}", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid UpdateScaleAsync({Id})", id);
                return false;
            }
        }

        /// <summary>
        /// Retrieves the next unanswered question in the assessment sequence.
        /// </summary>
        /// <param name="assessmentId">The current assessment ID.</param>
        /// <returns>The next Question object, or null if the assessment is complete or an error occurs.</returns>
        public async Task<Question?> GetNextQuestionAsync(int assessmentId)
        {
            try
            {
                // Delegating the "next question" calculation to the API keeps the frontend completely stateless.
                // It ensures a single source of truth for progression logic, especially when conditions (like skipped questions) exist.
                var response = await _httpClient.GetAsync($"Question/quiz/patient/next/{assessmentId}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<Question>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetNextQuestionAsync({AssessmentId})", assessmentId);
                return null;
            }
        }

        /// <summary>
        /// Retrieves the preceding question based on the user's current position in the sequence.
        /// </summary>
        /// <param name="assessmentId">The current assessment ID.</param>
        /// <param name="order">The order index of the current question.</param>
        /// <returns>The previous Question object, or null if at the beginning or an error occurs.</returns>
        public async Task<Question?> GetPreviousQuestionAsync(int assessmentId, int order)
        {
            try
            {
                // The 'order' parameter is strictly required because the chronological sequence might be non-linear 
                // if the patient has previously skipped questions. We must navigate backwards relative to where they are now.
                var response = await _httpClient.GetAsync($"Question/quiz/patient/previous/{assessmentId}/{order}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<Question>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetPreviousQuestionAsync({AssessmentId}, {Order})", assessmentId, order);
                return null;
            }
        }

        /// <summary>
        /// Submits the patient's numerical answer and optional free-text comment for a specific question.
        /// </summary>
        /// <param name="itemId">The unique ID of the specific assessment item (question instance).</param>
        /// <param name="dto">The payload containing the answer data.</param>
        /// <returns>True if the submission was successful, otherwise false.</returns>
        public async Task<bool> SubmitAnswerAsync(int itemId, PatientAnswerDto dto)
        {
            try
            {
                // We use HTTP PUT here because the underlying AssessmentItem (the placeholder for the answer) 
                // is pre-generated in the database when the assessment is initially created. We are mutating an existing record, not POSTing a new one.
                var response = await _httpClient.PutAsJsonAsync($"AssessmentItem/patient/{itemId}", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid SubmitAnswerAsync({ItemId})", itemId);
                return false;
            }
        }

        /// <summary>
        /// Marks a specific assessment item as skipped by the user.
        /// </summary>
        /// <param name="itemId">The unique ID of the specific assessment item.</param>
        /// <returns>True if successfully skipped, otherwise false.</returns>
        public async Task<bool> SkipQuestionAsync(int itemId)
        {
            try
            {
                // An empty anonymous object `new { }` is sent because the PUT endpoint technically requires a JSON body to parse, 
                // even though the URL itself conveys the entire intended state change ("skip").
                var response = await _httpClient.PutAsJsonAsync($"AssessmentItem/skip/{itemId}", new { });
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid SkipQuestionAsync({ItemId})", itemId);
                return false;
            }
        }

        /// <summary>
        /// Retrieves the total number of questions in the assessment to facilitate UI progress bar calculations.
        /// </summary>
        /// <param name="assessmentId">The current assessment ID.</param>
        /// <returns>The total count of questions, or null if the request fails.</returns>
        public async Task<int?> GetTotalQuestionCountAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"assessment/{assessmentId}/question-count");

                if (!response.IsSuccessStatusCode)
                    return null;

                // The API returns a raw scalar string (e.g., "9") rather than a JSON object for this specific endpoint.
                // Therefore, we must read it as a string and parse it manually rather than using ReadFromJsonAsync.
                var content = await response.Content.ReadAsStringAsync();
                return int.TryParse(content, out var count) ? count : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetTotalQuestionCountAsync för assessment {AssessmentId}", assessmentId);
                return null;
            }
        }
    }
}