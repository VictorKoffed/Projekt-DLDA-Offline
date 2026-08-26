namespace DLDA.GUI.DTOs.Staff
{
    /// <summary>
    /// Represents a data transfer object encapsulating healthcare professional result summaries 
    /// and comparative assessment metrics for reporting and review dashboards.
    /// </summary>
    public class StaffResult
    {
        public int AssessmentId { get; set; }          // Unique primary key identifier referencing the assessment session
        public int UserId { get; set; }                // Foreign key account identifier referencing the evaluated patient
        public string? Username { get; set; }          // Unique username handle for the patient account
        public string? ScaleType { get; set; }         // Defines the rating scale format configured for the assessment session
        public DateTime CreatedAt { get; set; }        // Timestamp recording when the assessment session was initially created

        public List<StaffResultRowDto> Questions { get; set; } = new();  // Collection of individual line item results and comparative scoring metrics for each question
    }

    /// <summary>
    /// Represents a granular data transfer object capturing line item comparison metrics 
    /// between patient self-assessments and clinical professional evaluations.
    /// </summary>
    public class StaffResultRowDto
    {
        public int ItemID { get; set; }                // Unique primary key identifier referencing the specific assessment line item instance
        public int Order { get; set; }                 // Sequence index determining the display position order within the professional review list
        public string QuestionText { get; set; } = string.Empty;  // Localized prompt text presented to the user, initialized to an empty string as a safe fallback default

        public int? PatientAnswer { get; set; }          // Numerical rating score submitted by the patient (nullable if unanswered or skipped)
        public int? StaffAnswer { get; set; }            // Numerical rating score evaluated and submitted by clinical staff (nullable if pending review)
        public string? PatientComment { get; set; }      // Optional contextual feedback commentary provided by the patient
        public string? StaffComment { get; set; }        // Optional clinical commentary or feedback provided by the professional reviewer
        public bool Flag { get; set; }                   // Indicator flag set by clinical staff to highlight items requiring further discussion or medical attention
        public bool SkippedByPatient { get; set; }       // Tracks whether the questionnaire item was intentionally bypassed by the patient

        // Automatically computes the absolute variance delta between patient ratings and clinical evaluations to highlight discrepancies
        public int Difference =>
            PatientAnswer.HasValue && StaffAnswer.HasValue
                ? Math.Abs(PatientAnswer.Value - StaffAnswer.Value)
                : -1;                           // Sentinel fallback value (-1) returned if either rating score is missing or unassigned
    }

    /// <summary>
    /// Represents an overview data transfer object tracking professional review completion states 
    /// alongside complete line item comparison datasets for staff review interfaces.
    /// </summary>
    public class StaffResultOverviewDto
    {
        public int AssessmentId { get; set; }          // Unique primary key identifier referencing the assessment session
        public int UserId { get; set; }                // Foreign key account identifier referencing the evaluated patient
        public string? Username { get; set; }          // Unique username handle for the patient account
        public DateTime CreatedAt { get; set; }        // Timestamp recording when the assessment session was initially created
        public bool IsStaffComplete { get; set; }      // Indicates whether healthcare professionals have committed their final sign-off review completion

        public List<StaffResultRowDto> Questions { get; set; } = new();  // Collection of detailed line item comparison rows associated with the overview session
    }
}