using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.DTOs.User
{
    /// <summary>
    /// Represents the core data transfer object encapsulating user account profile details, 
    /// security clearance roles, and credential properties for administrative and identity management views.
    /// </summary>
    public class UserDto
    {
        public int UserID { get; set; }                      // Unique primary key identifier referencing the user account record in the persistence store
        public string Username { get; set; } = string.Empty; // Unique user login handle string, initialized to an empty string as a safe fallback default
        public string? Email { get; set; }                   // Registered electronic mail contact address associated with the account (nullable if unassigned)
        public string Role { get; set; } = string.Empty;     // Security clearance role identifier governing access permissions (e.g., admin, staff, patient), initialized to empty string as default
        public string? Password { get; set; }                // Encrypted or plain-text credential payload utilized during account provisioning or updates (nullable for standard profile retrievals)
    }
}