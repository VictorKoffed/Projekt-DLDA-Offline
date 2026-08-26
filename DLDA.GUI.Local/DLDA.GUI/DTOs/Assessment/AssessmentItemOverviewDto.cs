namespace DLDA.GUI.DTOs.Assessment
{
    /// <summary>
    /// Represents a lightweight data transfer object summarizing individual questionnaire item responses 
    /// and core prompt text for patient overview dashboards and summary views.
    /// </summary>
    public class AssessmentItemOverviewDto
    {
        public int ItemID { get; set; } // Unique primary key identifier for the specific assessment line item instance

        public int QuestionID { get; set; } // Foreign key identifier referencing the master template question definition

        public string QuestionText { get; set; } = string.Empty; // Localized prompt text presented to the user, initialized to empty string as a default fallback

        public int? PatientAnswer { get; set; } // Numerical rating score submitted by the patient (null if left unanswered)

        public bool Flag { get; set; } // Indicator flag highlighting items marked for special clinical review or discussion

        public string? PatientComment { get; set; } // Optional descriptive feedback commentary provided by the patient

        public bool SkippedByPatient { get; set; } // Tracks whether the patient intentionally chose to bypass this question item
    }
}