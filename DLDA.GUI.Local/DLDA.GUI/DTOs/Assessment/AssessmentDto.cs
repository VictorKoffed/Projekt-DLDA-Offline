using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.DTOs.Assessment
{
    /// <summary>
    /// Represents the core data transfer object for an assessment session container, 
    /// encapsulating metadata regarding progress tracking, scale choices, and completion state flags.
    /// </summary>
    public class AssessmentDto
    {
        public int AssessmentID { get; set; } // Unique primary key identifier for the assessment session instance

        public string? ScaleType { get; set; } // Defines the selected questionnaire rating scale format (e.g., Smiley, Likert scale)

        public bool IsComplete { get; set; } // Indicates whether the patient has finalized and locked their assessment submission

        public int UserId { get; set; } // Foreign key account identifier referencing the user associated with the assessment session

        public DateTime CreatedAt { get; set; } // Timestamp recording when the assessment container was provisioned

        public bool HasStarted { get; set; } // Tracks whether the user has interacted with or begun answering questionnaire items

        public int AnsweredCount { get; set; } // Counter tracking how many individual items have received a valid response

        public int TotalQuestions { get; set; } // Total count of active items scoped to the assessment questionnaire template

        public bool IsStaffComplete { get; set; } // Indicates whether healthcare professionals have signed off on their review evaluation
    }
}