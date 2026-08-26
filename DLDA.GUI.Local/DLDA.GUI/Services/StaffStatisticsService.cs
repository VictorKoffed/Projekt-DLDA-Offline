using DLDA.GUI.DTOs.Assessment;
using DLDA.GUI.DTOs.Staff;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Service class responsible for retrieving and aggregating statistics for the staff view.
    /// Acts as an orchestration layer to decouple the MVC controllers from the complexities of fetching, 
    /// parsing, and combining multiple data sources required for the analytical dashboards.
    /// </summary>
    public class StaffStatisticsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StaffStatisticsService> _logger;

        /// <summary>
        /// Initializes a new instance of the StaffStatisticsService.
        /// </summary>
        /// <param name="factory">Provides a pre-configured HttpClient. Using IHttpClientFactory prevents socket exhaustion and manages the DNS lifecycle automatically.</param>
        /// <param name="logger">Records network anomalies or parsing failures to support operational observability.</param>
        public StaffStatisticsService(IHttpClientFactory factory, ILogger<StaffStatisticsService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Retrieves comparison data between the patient's self-assessment and the staff's assessment, along with the assessment metadata.
        /// Returning a Tuple reduces the number of service calls required by the Controller, ensuring cohesive data is fetched together.
        /// </summary>
        public async Task<(List<StaffStatistics>? Comparison, AssessmentDto? Assessment)> GetComparisonAsync(int assessmentId)
        {
            try
            {
                // Fetch comparison data
                // We use ReadAsStringAsync followed by manual deserialization to explicitly apply PropertyNameCaseInsensitive.
                // This ensures robust mapping even if the API's JSON casing conventions change.
                var comparisonResponse = await _httpClient.GetAsync($"statistics/comparison-table-staff/{assessmentId}");
                List<StaffStatistics>? comparison = null;
                if (comparisonResponse.IsSuccessStatusCode)
                {
                    var comparisonJson = await comparisonResponse.Content.ReadAsStringAsync();
                    comparison = JsonSerializer.Deserialize<List<StaffStatistics>>(comparisonJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                // Fetch assessment metadata
                var assessmentResponse = await _httpClient.GetAsync($"assessment/{assessmentId}");
                AssessmentDto? assessment = null;
                if (assessmentResponse.IsSuccessStatusCode)
                {
                    var assessmentJson = await assessmentResponse.Content.ReadAsStringAsync();
                    assessment = JsonSerializer.Deserialize<AssessmentDto>(assessmentJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                return (comparison, assessment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Undantag i GetComparisonAsync({AssessmentId})", assessmentId);
                return (null, null);
            }
        }

        /// <summary>
        /// Retrieves longitudinal overview data tracking changes over time for a specific patient.
        /// </summary>
        public async Task<StaffChangeOverviewDto?> GetChangeOverviewAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"statistics/staff-change-overview/{userId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Misslyckades att hämta översiktsdata: {StatusCode}", response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                
                // The API returns a plain text string instead of a 4xx status or a structured JSON response 
                // when there are fewer than two assessments available. We intercept this specific string 
                // to prevent a fatal JsonException during deserialization.
                if (json.Contains("inte tillräckligt")) return null;

                var overview = JsonSerializer.Deserialize<StaffChangeOverviewDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return overview;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Undantag i GetChangeOverviewAsync({UserId})", userId);
                return null;
            }
        }

        /// <summary>
        /// Retrieves the patient's self-assessment answers for a specific assessment, used primarily for data aggregation and summary views.
        /// </summary>
        public async Task<List<StaffStatistics>> GetPatientAnswerSummaryAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"statistics/patient-answer-summary/{assessmentId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Misslyckades att hämta patientens svarssammanställning: {StatusCode}", response.StatusCode);
                    return new();
                }

                var json = await response.Content.ReadAsStringAsync();
                var stats = JsonSerializer.Deserialize<List<StaffStatistics>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Returning an empty collection (?? new()) instead of null is a Clean Code practice.
                // It eliminates redundant null-checks in the Controller and prevents Razor views from throwing NullReferenceExceptions when iterating.
                return stats ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Undantag i GetPatientAnswerSummaryAsync({AssessmentId})", assessmentId);
                return new();
            }
        }

        /// <summary>
        /// Compares two selected, completed staff assessments for a patient to identify trends and deviations in the caregiver's evaluations.
        /// </summary>
        public async Task<StaffChangeOverviewDto?> CompareAssessmentsAsync(int firstId, int secondId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"statistics/staff-compare-assessments/{firstId}/{secondId}");
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                
                // Safety check to handle API business rule validation where it returns text instead of JSON if data is insufficient.
                if (json.Contains("inte tillräckligt")) return null;

                return JsonSerializer.Deserialize<StaffChangeOverviewDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid CompareAssessmentsAsync({FirstId}, {SecondId})", firstId, secondId);
                return null;
            }
        }

        /// <summary>
        /// Compares two patient self-assessments and returns the delta/changes in the patient's own answers over time, aiding staff in tracking subjective progress.
        /// </summary>
        public async Task<PatientChangeOverviewForStaffDto?> ComparePatientAnswersForStaffAsync(int firstId, int secondId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"statistics/compare-patient-answers-for-staff/{firstId}/{secondId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Misslyckades att hämta jämförelse av patientens svar: {StatusCode}", response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PatientChangeOverviewForStaffDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid ComparePatientAnswersForStaffAsync({FirstId}, {SecondId})", firstId, secondId);
                return null;
            }
        }
    }
}