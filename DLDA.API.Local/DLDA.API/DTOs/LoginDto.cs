using Microsoft.AspNetCore.Mvc;

namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents the authentication credentials payload submitted from the client frontend to the API
    /// during the login verification workflow.
    /// </summary>
    public class LoginDto
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}