using DLDA.GUI.DTOs.Assessment;

namespace DLDA.GUI.DTOs.User
{
    /// <summary>
    /// Represents a data transfer object encapsulating patient account identity 
    /// alongside their most recent assessment status container and temporal metadata for staff dashboards.
    /// </summary>
    public class PatientWithAssessmentStatusDto
    {
        public int UserID { get; set; }                          // Unique primary key identifier referencing the patient user account

        public string Username { get; set; } = "";               // Unique user login handle for the patient account, initialized to an empty string as a safe fallback default

        public AssessmentDto? LastAssessment { get; set; }       // Encapsulates the complete data transfer object representing the patient's latest assessment session container (nullable if none exist)

        // Computed property extracting the creation timestamp directly from the latest assessment container instance
        public DateTime? LastAssessmentDate => LastAssessment?.CreatedAt;
    }
}