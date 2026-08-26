using DLDA.GUI.DTOs.User;
using System.Net.Http.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Service class responsible for handling admin-related API calls for user management.
    /// Acts as an abstraction layer, encapsulating all user CRUD operations to keep the MVC controllers 
    /// thin and completely decoupled from raw HTTP communication and infrastructure concerns.
    /// </summary>
    public class UserAdminService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserAdminService> _logger;

        /// <summary>
        /// Initializes a new instance of the UserAdminService.
        /// </summary>
        /// <param name="factory">Provides a pre-configured HttpClient. Utilizing IHttpClientFactory prevents socket exhaustion and manages DNS lifecycle automatically.</param>
        /// <param name="logger">Records network or parsing failures to support operational observability and debugging.</param>
        public UserAdminService(IHttpClientFactory factory, ILogger<UserAdminService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the complete catalog of users from the API.
        /// </summary>
        /// <returns>A list of user DTOs. Returns an empty collection on failure (?? new()) to ensure the admin UI degrades gracefully (e.g., rendering an empty table) rather than throwing a fatal exception.</returns>
        public async Task<List<UserDto>> GetAllAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<UserDto>>("User") ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid hämtning av användarlista.");
                return new();
            }
        }

        /// <summary>
        /// Retrieves a specific user based on their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>The user object, or null if the user does not exist or a network error occurs. Returning null delegates the "Not Found" UI logic to the Controller.</returns>
        public async Task<UserDto?> GetByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UserDto>($"User/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid hämtning av användare ID: {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Submits a newly constructed user profile to the API for persistence.
        /// </summary>
        /// <param name="user">The data transfer object containing the new user's details.</param>
        /// <returns>True if the creation was successful, otherwise false. This boolean return simplifies the Controller's logic for displaying success or error notifications to the administrator.</returns>
        public async Task<bool> CreateAsync(UserDto user)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("User", user);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid skapande av användare.");
                return false;
            }
        }

        /// <summary>
        /// Updates an existing user's properties in the backend.
        /// </summary>
        /// <param name="id">The unique identifier of the user being modified.</param>
        /// <param name="user">The updated user data.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        public async Task<bool> UpdateAsync(int id, UserDto user)
        {
            try
            {
                // The PUT method is utilized here to enforce idempotency. Submitting the exact same update multiple times 
                // will yield the same final state in the database, which is a safer approach for administrative data-entry operations.
                var response = await _httpClient.PutAsJsonAsync($"User/{id}", user);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid uppdatering av användare ID: {Id}", id);
                return false;
            }
        }

        /// <summary>
        /// Permanently deletes a user from the system based on their ID.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <returns>True if the deletion was successful, otherwise false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"User/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid borttagning av användare ID: {Id}", id);
                return false;
            }
        }
    }
}