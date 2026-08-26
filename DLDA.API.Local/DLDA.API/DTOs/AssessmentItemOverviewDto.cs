namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents an overview data transfer object for individual questionnaire items,
    /// providing summarized response states and flags optimized for patient and clinical summary interfaces.
    /// </summary>
    public class AssessmentItemOverviewDto
    {
        public int ItemID { get; set; }
        public int QuestionID { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int? PatientAnswer { get; set; } // Nullable representation used to explicitly distinguish unaddressed items from valid score inputs
        public bool Flag { get; set; }
        public string? PatientComment { get; set; }
        public bool SkippedByPatient { get; set; }
    }
}