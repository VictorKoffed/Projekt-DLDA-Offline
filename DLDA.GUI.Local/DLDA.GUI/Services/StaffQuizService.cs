using DLDA.GUI.DTOs.Staff;
using System.Net.Http.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Service class responsible for managing the staff's (healthcare professional's) assessment workflow.
    /// It acts as a stateless orchestrator, delegating progression logic (like which question comes next) 
    /// to the API, ensuring the frontend remains decoupled from the underlying clinical business rules.
    /// </summary>
    public class StaffQuizService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StaffQuizService> _logger;

        /// <summary>
        /// Initializes a new instance of the StaffQuizService.
        /// </summary>
        /// <param name="factory">Provides a pre-configured HttpClient. Relying on IHttpClientFactory mitigates socket exhaustion and handles DNS resolution updates automatically.</param>
        /// <param name="logger">Records network or parsing failures to support operational observability and debugging.</param>
        public StaffQuizService(IHttpClientFactory factory, ILogger<StaffQuizService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the next unanswered question in the staff's assessment sequence.
        /// By determining the "next" state on the server, we avoid duplicating complex skipping or routing logic in the frontend.
        /// </summary>
        /// <param name="assessmentId">The unique identifier of the assessment.</param>
        /// <returns>The next question DTO, or null if the assessment is complete or an error occurs.</returns>
        public async Task<StaffQuestionDto?> GetNextQuestionAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Question/quiz/staff/next/{assessmentId}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<StaffQuestionDto>();
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
        /// <param name="order">The order index of the current question. Required because the chronological sequence might be non-linear if questions were skipped.</param>
        /// <returns>The previous question DTO, or null if at the beginning or an error occurs.</returns>
        public async Task<StaffQuestionDto?> GetPreviousQuestionAsync(int assessmentId, int order)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Question/quiz/staff/previous/{assessmentId}/{order}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<StaffQuestionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetPreviousQuestionAsync({AssessmentId}, {Order})", assessmentId, order);
                return null;
            }
        }

        /// <summary>
        /// Submits the staff member's evaluation answer.
        /// </summary>
        /// <param name="dto">The data transfer object containing the answer, optional comment, and flag status.</param>
        /// <returns>True if the submission was successful, otherwise false. This boolean response simplifies controller flow regarding validation or redirection.</returns>
        public async Task<bool> SubmitAnswerAsync(SubmitStaffAnswerDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("Question/quiz/staff/submit", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid SubmitAnswerAsync för ItemID={ItemId}", dto.ItemID);
                return false;
            }
        }

        /// <summary>
        /// Submits a skipped state for a specific question, optionally including a comment and a flag for future review.
        /// </summary>
        /// <param name="itemId">The unique ID of the specific assessment item.</param>
        /// <param name="comment">An optional justification for skipping the question.</param>
        /// <param name="flag">Indicates whether this question should be flagged for later review with the patient.</param>
        /// <returns>True if successfully skipped, otherwise false.</returns>
        public async Task<bool> SkipQuestionAsync(int itemId, string? comment, bool flag)
        {
            // We reuse the SubmitStaffAnswerDto here to maintain a consistent API contract.
            // Explicitly setting Answer to null is the business rule trigger that tells the backend this item was deliberately skipped rather than answered.
            var dto = new SubmitStaffAnswerDto
            {
                ItemID = itemId,
                Answer = null, 
                Comment = comment,
                Flag = flag
            };

            var response = await _httpClient.PostAsJsonAsync("question/quiz/staff/submit", dto);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Retrieves the total number of questions allocated to the staff for a specific assessment.
        /// This is utilized by the frontend to accurately calculate and render progress bars without needing to fetch the entire question payload into memory.
        /// </summary>
        /// <param name="assessmentId">The unique identifier of the assessment.</param>
        /// <returns>The total count of questions, or null if the request fails.</returns>
        public async Task<int?> GetTotalQuestionCountForStaffAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"assessment/{assessmentId}/question-count");

                if (!response.IsSuccessStatusCode)
                    return null;

                // The API returns a raw scalar string (e.g., "9") rather than a structured JSON object for this endpoint.
                // Therefore, we must read the content as a string and parse it manually rather than using ReadFromJsonAsync.
                var content = await response.Content.ReadAsStringAsync();
                return int.TryParse(content, out var count) ? count : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetTotalQuestionCountForStaffAsync för assessment {AssessmentId}", assessmentId);
                return null;
            }
        }
    }
}