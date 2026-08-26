namespace DLDA.GUI.DTOs.Assessment
{
    /// <summary>
    /// Represents a comprehensive data transfer object encapsulating an assessment session 
    /// overview along with its associated collection of questionnaire item details.
    /// </summary>
    public class AssessmentOverviewDto
    {
        public int AssessmentId { get; set; } // Unique primary key identifier for the assessment session

        public string? ScaleType { get; set; } // Defines the selected rating scale format configured for the assessment (e.g., Smiley, Likert scale)

        public bool IsComplete { get; set; } // Indicates whether the assessment session has been finalized and locked by the patient

        public DateTime CreatedAt { get; set; } // Timestamp recording when the assessment container was initialized

        public List<QuestionOverviewDto> Questions { get; set; } = new List<QuestionOverviewDto>(); // Collection of detailed item response views associated with this assessment session
    }

    /// <summary>
    /// Represents a lightweight data transfer object capturing specific questionnaire item details 
    /// within an ongoing or completed assessment overview context.
    /// </summary>
    public class QuestionOverviewDto
    {
        public int ItemID { get; set; } // Unique primary key identifier for the specific assessment line item instance

        public int QuestionId { get; set; } // Foreign key identifier referencing the master template question definition

        public string? QuestionText { get; set; } // The localized prompt text presented to the user for this question item

        public int? PatientAnswer { get; set; } // Numerical rating score submitted by the patient (null if left unanswered)

        public string? PatientComment { get; set; } // Optional descriptive feedback commentary provided by the patient

        public int Order { get; set; } // Sequence index determining the display position order within the questionnaire wizard (e.g., 0-37)

        public int Total { get; set; } // Total count of items included in the questionnaire set to establish progress ratios (e.g., 38)
    }
}