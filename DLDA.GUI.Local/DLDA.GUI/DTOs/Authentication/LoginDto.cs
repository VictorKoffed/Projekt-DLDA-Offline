using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.DTOs.Authentication
{
    /// <summary>
    /// Represents the credential data transfer object encapsulating user login input payloads 
    /// transmitted from the frontend interface to the authentication API endpoint.
    /// </summary>
    public class LoginDto
    {
        public string Username { get; set; } = ""; // User login account identifier string, initialized to an empty string as a safe fallback default

        public string Password { get; set; } = ""; // Plain-text password credential input provided during authentication requests, initialized to an empty string as default
    }
}