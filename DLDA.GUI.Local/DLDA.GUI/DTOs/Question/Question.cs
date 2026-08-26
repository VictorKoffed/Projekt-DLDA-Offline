using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.DTOs.Question
{
    /// <summary>
    /// Represents the core data transfer object encapsulating questionnaire prompt details, 
    /// routing sequence metadata, and associated response states for interactive quiz views.
    /// </summary>
    public class Question
    {
        public int QuestionID { get; set; }          // Unique primary key identifier referencing the master question definition

        public int AssessmentID { get; set; }        // Foreign key identifier referencing the parent assessment session container

        public int ItemID { get; set; }              // Unique primary key identifier referencing the specific assessment line item instance

        public string QuestionText { get; set; } = string.Empty;   // Localized prompt text presented to the user, initialized to an empty string as a safe fallback default

        public string Category { get; set; } = string.Empty;      // Classification grouping category associated with the question item, initialized to empty string as default

        public bool IsActive { get; set; }           // Indicates whether the question template is currently active and eligible for inclusion in assessments

        public int Order { get; set; }               // Sequence index determining the display position order within the wizard questionnaire (e.g., 0-37)

        public int Total { get; set; }               // Total count of active questions included in the questionnaire set to establish progress indicators

        public string? ScaleType { get; set; }       // Defines the rating scale format configured for the question (e.g., Smiley, Likert scale), null if inherited or unassigned

        public int AssessmentItemID { get; set; }    // Duplicate foreign key reference identifier mapping the line item instance to the assessment

        public int? PatientAnswer { get; set; }      // Numerical rating score submitted by the patient for this item (nullable if left unanswered)

        public string? PatientComment { get; set; }  // Optional descriptive feedback commentary provided by the patient for this question
    }

    /// <summary>
    /// Represents the data transfer object payload used when flagging a specific questionnaire item as bypassed or skipped.
    /// </summary>
    public class SkipQuestionDto
    {
        public int ItemID { get; set; }            // Unique primary key identifier referencing the assessment line item to be flagged as skipped
    }
}