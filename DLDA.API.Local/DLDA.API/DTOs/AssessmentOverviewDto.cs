namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents a detailed assessment overview container bundling aggregate form metadata
    /// along with a structured sequence of question item sub-elements for client navigation.
    /// </summary>
    public class AssessmentOverviewDto
    {
        public int AssessmentId { get; set; }
        public string? ScaleType { get; set; }
        public bool IsComplete { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<QuestionOverviewDto> Questions { get; set; } = new List<QuestionOverviewDto>();
    }

    /// <summary>
    /// Represents an individual question entry within an assessment overview,
    /// tracking localized response details, sequence positioning, and total scope metrics for progress indicators.
    /// </summary>
    public class QuestionOverviewDto
    {
        public int ItemID { get; set; }
        public int QuestionId { get; set; }
        public string? QuestionText { get; set; }
        public int? PatientAnswer { get; set; }  // Nullable representation used to explicitly distinguish unaddressed items from valid score inputs
        public string? PatientComment { get; set; }
        public int Order { get; set; } // Sequential index position within the wizard flow (e.g., 0-37)
        public int Total { get; set; } // Total count scope baseline used for step progress calculations (e.g., 38)
    }
}