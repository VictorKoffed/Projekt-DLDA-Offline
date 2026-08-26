namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents the fundamental data transfer object for user account profiles,
    /// encapsulating identity properties, role permissions, and optional password payloads for creation or updates.
    /// </summary>
    public class UserDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? Password { get; set; }
    }
}