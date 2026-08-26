namespace DLDA.API.DTOs.Patient
{
    /// <summary>
    /// Represents a patient user entity enriched with status indicators and details of their most recent assessment,
    /// designed for clinical dashboards and patient directory overviews.
    /// </summary>
    public class PatientWithAssessmentStatusDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = "";

        public AssessmentDto? LastAssessment { get; set; }

        public DateTime? LastAssessmentDate => LastAssessment?.CreatedAt;
    }
}