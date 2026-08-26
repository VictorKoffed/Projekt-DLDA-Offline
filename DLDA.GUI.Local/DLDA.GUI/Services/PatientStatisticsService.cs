using DLDA.GUI.DTOs.Assessment;
using DLDA.GUI.DTOs.Patient;
using System.Net.Http.Json;
using System.Text.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Service class responsible for retrieving and aggregating patient statistics.
    /// Acts as an abstraction layer to decouple the MVC controllers and Razor views from raw HTTP communication and data parsing.
    /// </summary>
    public class PatientStatisticsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PatientStatisticsService> _logger;

        /// <summary>
        /// Initializes a new instance of the PatientStatisticsService.
        /// </summary>
        /// <param name="factory">Provides a pre-configured HttpClient. Using IHttpClientFactory prevents socket exhaustion and manages DNS lifecycle automatically.</param>
        /// <param name="logger">Logger instance for capturing API integration failures and unexpected runtime exceptions.</param>
        public PatientStatisticsService(IHttpClientFactory factory, ILogger<PatientStatisticsService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all answers submitted by a patient for a specific assessment.
        /// </summary>
        /// <param name="assessmentId">The unique identifier of the assessment.</param>
        /// <returns>A list of patient answers. Returns an empty list on failure to prevent NullReferenceExceptions in the UI layer.</returns>
        public async Task<List<PatientAnswerStatsDto>> GetAnswersForAssessmentAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"AssessmentItem/patient/assessment/{assessmentId}");
                if (!response.IsSuccessStatusCode) return new();

                // Returning an empty collection (?? new()) instead of null is a Clean Code practice. 
                // It eliminates the need for redundant null-checks in the Controller and prevents Razor views from crashing when iterating.
                return await response.Content.ReadFromJsonAsync<List<PatientAnswerStatsDto>>() ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetAnswersForAssessmentAsync({AssessmentId})", assessmentId);
                return new();
            }
        }

        /// <summary>
        /// Retrieves fundamental metadata for a specific assessment.
        /// </summary>
        /// <param name="assessmentId">The unique identifier of the assessment.</param>
        /// <returns>The assessment metadata, or null if the request fails, allowing the caller to handle missing data explicitly.</returns>
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
        /// Retrieves an aggregated statistical summary for a single assessment.
        /// </summary>
        /// <param name="assessmentId">The unique identifier of the assessment.</param>
        /// <returns>A DTO containing the statistical summary, or null if the API call fails.</returns>
        public async Task<PatientSingleSummaryDto?> GetSummaryAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"statistics/summary/patient/{assessmentId}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<PatientSingleSummaryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetSummaryAsync({AssessmentId})", assessmentId);
                return null;
            }
        }

        /// <summary>
        /// Retrieves longitudinal improvement data for a patient to track progress over time.
        /// A minimum of two completed assessments is required by the API to calculate this data.
        /// </summary>
        /// <param name="userId">The unique identifier of the patient.</param>
        /// <returns>A DTO containing the change overview, or null if data is insufficient or the request fails.</returns>
        public async Task<PatientChangeOverviewDto?> GetImprovementDataAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"statistics/patient-change-overview/{userId}");
                if (!response.IsSuccessStatusCode) return null;

                // We read the content as a raw string first. 
                // The API currently returns a success status with a plain text message if there aren't enough assessments, 
                // rather than returning a structured JSON or a 4xx status. 
                // We intercept this specific string to prevent JSON deserialization exceptions.
                var json = await response.Content.ReadAsStringAsync();
                if (json.Contains("inte tillräckligt")) return null;

                return JsonSerializer.Deserialize<PatientChangeOverviewDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetImprovementDataAsync({UserId})", userId);
                return null;
            }
        }

        /// <summary>
        /// Calculates the statistical differences between two specific assessments to visualize changes.
        /// </summary>
        /// <param name="id1">The unique identifier of the first assessment.</param>
        /// <param name="id2">The unique identifier of the second assessment.</param>
        /// <returns>A DTO containing the comparison metrics, or null if the API call fails.</returns>
        public async Task<PatientChangeOverviewDto?> CompareAssessmentsAsync(int id1, int id2)
        {
            try
            {
                var response = await _httpClient.GetAsync($"statistics/compare-assessments/{id1}/{id2}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<PatientChangeOverviewDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid CompareAssessmentsAsync({Id1}, {Id2})", id1, id2);
                return null;
            }
        }
    }
}