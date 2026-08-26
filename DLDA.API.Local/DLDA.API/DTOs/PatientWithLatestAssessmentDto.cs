namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents a simplified projection for healthcare staff views, exposing patient names
    /// alongside the timestamp of their most recent clinical assessment.
    /// </summary>
    public class PatientWithLatestAssessmentDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = "";
        public DateTime? LastAssessmentDate { get; set; }
    }
}