using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.DTOs.Authentication
{
    /// <summary>
    /// Represents the data transfer object containing user identity claims and security role tokens 
    /// returned by the authentication service upon successful credential validation.
    /// </summary>
    public class AuthResponseDto
    {
        public int UserID { get; set; } // Unique primary key identifier for the authenticated user account

        public string Username { get; set; } = ""; // Unique user login handle, initialized to an empty string as a safe fallback default

        public string Role { get; set; } = ""; // Security clearance role identifier assigned to the user session (e.g., admin, staff, patient), initialized to empty string as default
    }
}