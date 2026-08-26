namespace DLDA.GUI.DTOs.Assessment
{
    /// <summary>
    /// Represents a data transfer object containing patient identity details 
    /// alongside tracking metrics for their most recent assessment session, designed for clinical dashboards.
    /// </summary>
    public class PatientWithLatestAssessmentDto
    {
        public int UserID { get; set; } // Unique primary key identifier referencing the patient user account

        public string Username { get; set; } = ""; // Unique username handle for the patient, initialized to an empty string as a safe fallback default

        public DateTime? LastAssessmentDate { get; set; } // Timestamp recording when the patient's most recent assessment was created (nullable if no historical sessions exist)
    }
}