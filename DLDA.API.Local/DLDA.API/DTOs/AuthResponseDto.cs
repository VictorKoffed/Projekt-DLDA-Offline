namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents the response payload returned by the authentication API upon successful user verification,
    /// supplying essential identity context and role authorizations required for client session management.
    /// </summary>
    public class AuthResponseDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = "";
        public string Role { get; set; } = "";
    }
}