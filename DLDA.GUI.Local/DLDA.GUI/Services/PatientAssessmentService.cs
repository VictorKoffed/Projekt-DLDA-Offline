using DLDA.GUI.DTOs.Assessment;
using System.Net.Http.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Service class responsible for retrieving patient assessments via the DLDA API.
    /// Acts as an abstraction layer to decouple the MVC controllers and Razor views from raw HTTP communication.
    /// </summary>
    public class PatientAssessmentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PatientAssessmentService> _logger;

        /// <summary>
        /// Initializes a new instance of the PatientAssessmentService.
        /// </summary>
        /// <param name="factory">Used to create the HttpClient. Relying on IHttpClientFactory prevents socket exhaustion and manages DNS changes automatically.</param>
        /// <param name="logger">Logger instance for capturing API integration failures and unexpected runtime exceptions.</param>
        public PatientAssessmentService(IHttpClientFactory factory, ILogger<PatientAssessmentService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all historical and current assessments for a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user (patient).</param>
        /// <returns>A list of AssessmentDto objects. Returns an empty list if the request fails or no data is found, ensuring the UI layer doesn't crash on null references.</returns>
        public async Task<List<AssessmentDto>> GetAssessmentsForUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Assessment/user/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    // We log the error for monitoring but do not throw an exception to the caller.
                    // This allows the UI to degrade gracefully and simply present an empty state to the user instead of a yellow screen of death.
                    _logger.LogError("API-svar misslyckades vid hämtning av bedömningar för patient {UserId}. Status: {StatusCode}",
                        userId, response.StatusCode);
                    return new List<AssessmentDto>();
                }

                var result = await response.Content.ReadFromJsonAsync<List<AssessmentDto>>();
                
                // Using the null-coalescing operator to guarantee a non-null return value.
                // Returning an empty collection instead of null is a Clean Code practice that prevents NullReferenceExceptions in Razor views and eliminates redundant null-checks in the Controller.
                return result ?? new List<AssessmentDto>();
            }
            catch (Exception ex)
            {
                // Catching a broad exception here prevents transient network issues, DNS failures, or serialization faults from crashing the application.
                _logger.LogError(ex, "Fel vid API-anrop: GetAssessmentsForUserAsync({UserId})", userId);
                return new List<AssessmentDto>();
            }
        }
    }
}