using Microsoft.EntityFrameworkCore;

namespace DLDA.API.Models
{
    /// <summary>
    /// Represents a system user entity in the database, storing authentication credentials,
    /// role-based security assignments, and relational assessment session histories.
    /// </summary>
    [PrimaryKey(nameof(UserID))]
    public class User
    {
        // Primary key uniquely identifying the system user account
        public int UserID { get; set; }
        // Unique username handle used for login identification and display interfaces
        public string Username { get; set; } = string.Empty;
        // Optional electronic mail address associated with the user account
        public string? Email { get; set; } = string.Empty;
        // Cryptographically secured BCrypt password hash ensuring safe credential storage
        public string PasswordHash { get; set; } = string.Empty;
        // Security role defining system access privileges and view authorizations (e.g., Patient, Staff, Admin)
        public string Role { get; set; } = string.Empty;
        // Timestamp recording when the user account profile was initially provisioned
        public DateTime CreatedAt { get; set; }

        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    }
}