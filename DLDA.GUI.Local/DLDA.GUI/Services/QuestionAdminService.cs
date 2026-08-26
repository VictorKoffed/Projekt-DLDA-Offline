using DLDA.GUI.DTOs.Question;
using System.Net.Http.Json;

/// <summary>
/// Service class responsible for managing admin-related API calls for assessment questions.
/// Encapsulates all CRUD operations, ensuring the presentation layer (Controllers) remains 
/// decoupled from raw HTTP communication and infrastructure concerns.
/// </summary>
public class QuestionAdminService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<QuestionAdminService> _logger;

    /// <summary>
    /// Initializes a new instance of the QuestionAdminService.
    /// </summary>
    /// <param name="factory">Provides a pre-configured HttpClient. Relying on IHttpClientFactory mitigates socket exhaustion and handles DNS resolution updates automatically.</param>
    /// <param name="logger">Records network or parsing failures to support operational observability.</param>
    public QuestionAdminService(IHttpClientFactory factory, ILogger<QuestionAdminService> logger)
    {
        _httpClient = factory.CreateClient("DLDA");
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the complete catalog of assessment questions from the API.
    /// </summary>
    /// <returns>A list of questions. Returns an empty list on failure to ensure the admin UI degrades gracefully (e.g., rendering an empty table) rather than throwing a fatal exception.</returns>
    public async Task<List<Question>> GetAllQuestionsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("Question");
            
            // EnsureSuccessStatusCode throws an HttpRequestException if the response is unsuccessful.
            // This immediately transfers control flow to the catch block, guaranteeing that we log the exact failure
            // and return a safe, empty collection instead of attempting to parse an error payload.
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<List<Question>>() ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid hämtning av alla frågor.");
            return new();
        }
    }

    /// <summary>
    /// Retrieves a specific question by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the question.</param>
    /// <returns>The question object, or null if it does not exist or a network error occurs. Returning null delegates the "Not Found" UI logic to the Controller.</returns>
    public async Task<Question?> GetQuestionByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"Question/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Question>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid hämtning av fråga ID {Id}.", id);
            return null;
        }
    }

    /// <summary>
    /// Submits a newly constructed question to the API for persistence.
    /// </summary>
    /// <param name="dto">The data transfer object containing the new question's details.</param>
    /// <returns>True if the creation was successful, otherwise false. This boolean return simplifies the Controller's logic for displaying success or error notifications to the administrator.</returns>
    public async Task<bool> CreateQuestionAsync(Question dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("Question", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid skapande av fråga.");
            return false;
        }
    }

    /// <summary>
    /// Updates an existing question's properties in the backend.
    /// </summary>
    /// <param name="id">The unique identifier of the question being modified.</param>
    /// <param name="dto">The updated question data.</param>
    /// <returns>True if the update was successful, otherwise false.</returns>
    public async Task<bool> UpdateQuestionAsync(int id, Question dto)
    {
        try
        {
            // The PUT method is used here to enforce idempotency. Sending the same update multiple times 
            // will yield the same final state in the database, which is safer for admin data-entry operations.
            var response = await _httpClient.PutAsJsonAsync($"Question/{id}", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid uppdatering av fråga ID {Id}.", id);
            return false;
        }
    }

    /// <summary>
    /// Deletes a question from the system based on its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the question to delete.</param>
    /// <returns>True if the deletion was successful, otherwise false.</returns>
    public async Task<bool> DeleteQuestionAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"Question/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid borttagning av fråga ID {Id}.", id);
            return false;
        }
    }
}