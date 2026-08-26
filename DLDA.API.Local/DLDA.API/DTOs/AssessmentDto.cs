namespace DLDA.API.DTOs
{
    /// <summary>
    /// Data Transfer Object representing an assessment container, its configuration parameters, 
    /// ownership mapping, and progress tracking metrics for UI rendering.
    /// </summary>
    public class AssessmentDto
    {
        public int AssessmentID { get; set; }
        public string? ScaleType { get; set; }
        public bool IsComplete { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasStarted { get; set; }
        public int AnsweredCount { get; set; }   // Tracks the number of questions addressed to compute completion progress bars
        public int TotalQuestions { get; set; }  // Represents the full scope baseline required for ratio calculations
        public bool IsStaffComplete { get; set; } // Tracks clinical sign-off status independent of patient completion flags
    }
}