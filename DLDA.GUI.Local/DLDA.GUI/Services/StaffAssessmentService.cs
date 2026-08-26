using DLDA.GUI.DTOs.Assessment;
using DLDA.GUI.DTOs.User;
using System.Net.Http.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Service class responsible for managing staff (healthcare professional) access to patient data and assessments.
    /// Acts as an orchestration layer for the MVC controllers, centralizing business rules and decoupling the presentation layer from raw HTTP communication.
    /// </summary>
    public class StaffAssessmentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StaffAssessmentService> _logger;

        /// <summary>
        /// Initializes a new instance of the StaffAssessmentService.
        /// </summary>
        /// <param name="factory">Provides a pre-configured HttpClient. Relying on IHttpClientFactory mitigates socket exhaustion and handles DNS resolution updates automatically.</param>
        /// <param name="logger">Records network or parsing failures to support operational observability and debugging.</param>
        public StaffAssessmentService(IHttpClientFactory factory, ILogger<StaffAssessmentService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a list of all patients alongside their most recent assessment status.
        /// Primarily used to populate the staff dashboard, enabling healthcare professionals to quickly identify patients needing attention.
        /// </summary>
        /// <returns>A list of patient data. Returns an empty collection on failure to ensure the UI grid renders gracefully without throwing exceptions.</returns>
        public async Task<List<PatientWithLatestAssessmentDto>> GetPatientsWithLatestAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<PatientWithLatestAssessmentDto>>("User/with-latest-assessment") ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid hämtning av patientlista.");
                return new();
            }
        }

        /// <summary>
        /// Retrieves the username for a specific user ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>The username, or a localized default fallback string if the user is not found or a network error occurs.</returns>
        public async Task<string> GetUsernameAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"User/{userId}");
                
                // We return a safe, localized fallback ("Okänt namn") instead of throwing or returning null.
                // This guarantees the Razor views always have a renderable string, avoiding NullReferenceExceptions in the UI.
                if (!response.IsSuccessStatusCode) return "Okänt namn";

                var user = await response.Content.ReadFromJsonAsync<UserDto>();
                return user?.Username ?? "Okänt namn";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid hämtning av användare {UserId}", userId);
                return "Okänt namn";
            }
        }

        /// <summary>
        /// Retrieves the historical list of all assessments for a specific patient.
        /// </summary>
        /// <param name="userId">The unique identifier of the patient.</param>
        /// <returns>A collection of assessments. Returns an empty list on failure.</returns>
        public async Task<List<AssessmentDto>> GetAssessmentsForUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Assessment/user/{userId}");
                if (!response.IsSuccessStatusCode) return new();

                return await response.Content.ReadFromJsonAsync<List<AssessmentDto>>() ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid hämtning av bedömningar för användare {UserId}", userId);
                return new();
            }
        }

        /// <summary>
        /// Initializes a new assessment session for a patient.
        /// </summary>
        /// <param name="userId">The unique identifier of the patient receiving the new assessment.</param>
        /// <returns>True if the creation was successful, otherwise false.</returns>
        public async Task<bool> CreateAssessmentAsync(int userId)
        {
            // We explicitly define the initial state of the assessment here.
            // Setting the default scale to "Numerisk" and IsComplete to false prepares the baseline state 
            // required by the database before the patient or staff begins answering questions.
            var dto = new AssessmentDto
            {
                UserId = userId,
                ScaleType = "Numerisk",
                IsComplete = false
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("Assessment", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid skapande av ny bedömning.");
                return false;
            }
        }

        /// <summary>
        /// Retrieves a specific assessment by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the assessment.</param>
        /// <returns>The assessment metadata, or null if it cannot be found.</returns>
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
        /// Permanently deletes an assessment based on its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the assessment to delete.</param>
        /// <returns>True if the deletion was successful, otherwise false.</returns>
        public async Task<bool> DeleteAssessmentAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Assessment/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid DeleteAssessmentAsync({Id})", id);
                return false;
            }
        }

        /// <summary>
        /// Searches for patients based on a free-text search string.
        /// </summary>
        /// <param name="search">The search term provided by the user.</param>
        /// <returns>A list of matching user profiles.</returns>
        public async Task<List<UserDto>> SearchPatientsAsync(string? search)
        {
            try
            {
                // Uri.EscapeDataString is strictly required here to sanitize the user input.
                // It ensures that spaces, special characters, or malicious inputs don't result in malformed HTTP requests.
                var endpoint = string.IsNullOrWhiteSpace(search)
                    ? "user/patients"
                    : $"user/patients?search={Uri.EscapeDataString(search)}";

                var response = await _httpClient.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                    return new List<UserDto>();

                var result = await response.Content.ReadFromJsonAsync<List<UserDto>>();
                return result ?? new List<UserDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid sökning av patienter");
                return new List<UserDto>();
            }
        }

        /// <summary>
        /// Retrieves a list of patients alongside their most recent assessment status, 
        /// utilizing dynamic query parameters to filter by search terms, ongoing status, and date intervals.
        /// </summary>
        /// <param name="search">Optional text to search for specific patient names or identifiers.</param>
        /// <param name="ongoing">If true, filters for patients with an ongoing (incomplete) assessment.</param>
        /// <param name="notOngoing">If true, filters for patients without an ongoing assessment.</param>
        /// <param name="recent">A date string or interval identifier to filter recent activity.</param>
        /// <returns>A filtered list of patient status objects.</returns>
        public async Task<List<PatientWithAssessmentStatusDto>> GetFilteredPatientsAsync(
            string? search, bool? ongoing, bool? notOngoing, string? recent)
        {
            try
            {
                // We construct the query string dynamically based on the provided parameters.
                // Pushing this filtering logic to the API (backend) rather than filtering a large dataset in memory (frontend)
                // drastically reduces network payload size and improves overall application performance.
                var queryParams = new List<string>();

                if (!string.IsNullOrWhiteSpace(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");

                if (ongoing == true)
                    queryParams.Add("ongoing=true");

                if (notOngoing == true)
                    queryParams.Add("notOngoing=true");

                if (!string.IsNullOrWhiteSpace(recent))
                    queryParams.Add($"recent={Uri.EscapeDataString(recent)}");

                var query = queryParams.Any()
                    ? "user/with-latest-assessment?" + string.Join("&", queryParams)
                    : "user/with-latest-assessment";

                var response = await _httpClient.GetAsync(query);
                if (!response.IsSuccessStatusCode)
                    return new List<PatientWithAssessmentStatusDto>();

                var result = await response.Content.ReadFromJsonAsync<List<PatientWithAssessmentStatusDto>>();
                return result ?? new List<PatientWithAssessmentStatusDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid hämtning av filtrerad patientlista.");
                return new List<PatientWithAssessmentStatusDto>();
            }
        }
    }
}