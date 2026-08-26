namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents a comprehensive data transfer object linking questionnaire templates to specific assessments,
    /// encapsulating both patient responses and clinical evaluations, comments, and risk flags for administrative oversight.
    /// </summary>
    public class AssessmentItemDto
    {
        public int ItemID { get; set; }
        public int AssessmentID { get; set; }
        public int QuestionID { get; set; }

        public int? PatientAnswer { get; set; }
        public string? PatientComment { get; set; }

        public int? StaffAnswer { get; set; }
        public string? StaffComment { get; set; }
        public int Order { get; set; }
        public bool Flag { get; set; }
        public bool SkippedByPatient { get; set; }
    }
}