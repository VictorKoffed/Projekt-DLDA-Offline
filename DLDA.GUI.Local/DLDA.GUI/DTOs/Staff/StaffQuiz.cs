namespace DLDA.GUI.DTOs.Staff
{
    /// <summary>
    /// Represents a comprehensive data transfer object encapsulating questionnaire item details, 
    /// patient responses, and clinical evaluation metrics from the healthcare professional perspective.
    /// </summary>
    public class StaffQuestionDto
    {
        public int ItemID { get; set; }              // Unique primary key identifier referencing the specific assessment line item instance (utilized for PUT mutation operations)
        public int UserID { get; set; }              // Foreign key account identifier referencing the patient associated with the session
        public int AssessmentID { get; set; }        // Foreign key identifier referencing the parent assessment session container
        public int QuestionID { get; set; }        // Foreign key identifier referencing the master template question definition
        public string QuestionText { get; set; } = string.Empty;    // Localized prompt text presented to the user, initialized to an empty string as a safe fallback default
        public string Category { get; set; } = string.Empty;       // Classification grouping category associated with the question item, initialized to empty string as default
        public int Order { get; set; }               // Sequence index determining the display position order within the professional review list
        public int Total { get; set; }               // Total count of active questions included in the questionnaire set
        public string? ScaleType { get; set; }       // Defines the rating scale format configured for the assessment, nullable if unassigned

        // Patient response metrics and commentary inputs
        public int? PatientAnswer { get; set; }      // Numerical rating score submitted by the patient (nullable if left unanswered)
        public string? PatientComment { get; set; } // Optional contextual feedback commentary provided by the patient (nullable if none exists)

        // Healthcare professional evaluation metrics and feedback commentary
        public int? StaffAnswer { get; set; }        // Numerical rating score evaluated and submitted by clinical staff (nullable if pending review)
        public string? StaffComment { get; set; }   // Optional clinical commentary or feedback provided by the professional reviewer (nullable if none exists)

        // Clinical risk review flag indicators
        public bool Flag { get; set; }               // Indicator flag set by clinical staff to highlight items requiring further discussion or medical attention

        // Patient identity properties
        public string PatientName { get; set; } = string.Empty;  // Unique name handle for the patient, initialized to an empty string as a safe fallback default
    }

    /// <summary>
    /// Represents a data transfer object capturing professional evaluation inputs, 
    /// clinical commentary, and risk flags during staff assessment reviews.
    /// </summary>
    public class StaffQuiz
    {
        public int? Answer { get; set; }     // Numerical score rating evaluated by the healthcare professional (nullable if pending)

        public string? Comment { get; set; }  // Optional clinical commentary or notes provided by the professional reviewer (nullable if none exists)

        public bool? Flag { get; set; }       // Indicator flag marking the item for special follow-up actions or clinical review (nullable)
    }

    /// <summary>
    /// Represents the submission payload data transfer object used when transmitting 
    /// professional evaluation scores, risk flags, and clinical comments to the server backend.
    /// </summary>
    public class SubmitStaffAnswerDto
    {
        public int ItemID { get; set; }            // Unique primary key identifier referencing the assessment line item being updated
        public int? Answer { get; set; }           // Numerical score rating submitted by staff, typically scaled within defined questionnaire boundaries (e.g., 0 to 4)
        public bool? Flag { get; set; }            // Risk indicator flag marking the item for mandatory clinical follow-up or discussion
        public string? Comment { get; set; }       // Optional clinical feedback commentary submitted alongside the evaluation score
    }
}