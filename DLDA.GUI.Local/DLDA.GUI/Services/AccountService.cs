using DLDA.GUI.DTOs.Authentication;
using System.Net.Http.Json;

/// <summary>
/// Service responsible for handling account-related communication with the DLDA.API.
/// Acts as the central integration point for the frontend's authentication flow, 
/// abstracting away the raw HTTP requests to ensure the UI controllers remain thin and strictly focused on presentation.
/// </summary>
public class AccountService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccountService> _logger;

    /// <summary>
    /// Initializes a new instance of the AccountService.
    /// </summary>
    /// <param name="factory">Used to retrieve the pre-configured HttpClient for DLDA.API. Relying on the IHttpClientFactory pattern prevents socket exhaustion and centralizes DNS rotation management.</param>
    /// <param name="logger">Logger instance for capturing runtime anomalies and authentication failures.</param>
    public AccountService(IHttpClientFactory factory, ILogger<AccountService> logger)
    {
        _httpClient = factory.CreateClient("DLDA");
        _logger = logger;
    }

    /// <summary>
    /// Attempts to authenticate a user via the DLDA.API.
    /// </summary>
    /// <param name="login">The data transfer object containing the user's credentials.</param>
    /// <returns>An AuthResponseDto containing the user's session data if successful; otherwise, null.</returns>
    public async Task<AuthResponseDto?> LoginAsync(LoginDto login)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/login", login);

            // We return null upon failure instead of throwing an exception to enforce a predictable control flow in the GUI layer.
            // The presentation layer (Controller) is responsible for interpreting a null result as an "invalid credentials" scenario and providing user feedback.
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Inloggning misslyckades. Status: {Status}", response.StatusCode);
                return null;
            }

            var user = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            
            // Deserialization failure implies a contract breach between the API and GUI data models.
            // This is a critical infrastructure discrepancy, which is why it is logged as an Error rather than a standard Warning.
            if (user == null)
                _logger.LogError("Inloggningssvaret kunde inte deserialiseras.");

            return user;
        }
        catch (Exception ex)
        {
            // Catching a generic Exception ensures that unexpected network failures (e.g., transient connection drops or API downtime) 
            // do not crash the web application, allowing the UI to degrade gracefully and display a friendly fallback message.
            _logger.LogError(ex, "Fel vid API-anrop under inloggning.");
            return null;
        }
    }
}