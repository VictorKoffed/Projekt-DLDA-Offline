namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents a specialized data transfer object encapsulating questionnaire item details,
    /// patient responses, and clinical evaluation inputs for the healthcare professional's review wizard interface.
    /// </summary>
    public class StaffQuestionDto
    {
        public int ItemID { get; set; }              // Required for targeted entity updates during professional PUT requests
        public int UserID { get; set; }
        public int AssessmentID { get; set; }
        public int QuestionID { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Order { get; set; }
        public int Total { get; set; }
        public string? ScaleType { get; set; }

        // Patient response data providing self-assessment context for clinical comparison
        public int? PatientAnswer { get; set; }
        public string? PatientComment { get; set; }

        // Healthcare professional evaluation response data
        public int? StaffAnswer { get; set; }
        public string? StaffComment { get; set; }

        // Risk flag indicating items requiring follow-up discussions or special clinical focus
        public bool Flag { get; set; }

        // Patient identity handle displayed in the clinical review header
        public string PatientName { get; set; } = string.Empty;
    }
}