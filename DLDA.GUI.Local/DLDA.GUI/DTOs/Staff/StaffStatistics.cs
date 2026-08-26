using DLDA.GUI.DTOs.Patient;  // Imports patient answer transfer objects for evaluation mapping

namespace DLDA.GUI.DTOs.Staff
{
    /// <summary>
    /// Represents a comparative data transfer object contrasting patient self-assessment responses 
    /// against healthcare professional evaluations for analytical reporting and clinical review views.
    /// </summary>
    public class StaffStatistics
    {
        public int QuestionNumber { get; set; }      // Sequence display order number for the questionnaire item
        public string QuestionText { get; set; } = string.Empty;    // Localized prompt text for the question, initialized to an empty string as a safe fallback default
        public string Category { get; set; } = string.Empty;       // Classification grouping category associated with the question item, initialized to empty string as default

        // Patient response metrics
        public int? PatientAnswer { get; set; }      // Numerical rating score submitted by the patient (nullable if unanswered)
        public string? PatientComment { get; set; }  // Optional descriptive feedback commentary provided by the patient (nullable if none exists)

        // Healthcare professional evaluation metrics
        public int? StaffAnswer { get; set; }        // Numerical rating score evaluated and submitted by clinical staff (nullable if pending)
        public string? StaffComment { get; set; }    // Optional clinical commentary or feedback notes provided by staff (nullable if none exists)

        // Analytical classification tags (e.g., match, mild-diff, strong-diff, skipped)
        public string Classification { get; set; } = string.Empty;  // Categorized variance classification tag grouping discrepancy levels between patient and staff scores

        // Patient response behavioral flags
        public bool SkippedByPatient { get; set; }   // Tracks whether the questionnaire item was intentionally bypassed by the patient

        // Clinical risk review flags
        public bool IsFlagged { get; set; }          // Indicator flag marking the item for mandatory clinical follow-up or discussion by staff

        // Session metadata properties
        public DateTime CreatedAt { get; set; }      // Timestamp recording when the comparison dataset instance was generated
        public string Username { get; set; } = string.Empty;  // Unique user login handle for the patient account, initialized to an empty string as default
    }

    /// <summary>
    /// Represents an overview data transfer object encapsulating longitudinal score shifts, 
    /// clinical risk flags, and behavioral changes across multiple sessions from a staff perspective.
    /// </summary>
    public class StaffChangeOverviewDto
    {
        public string Username { get; set; } = string.Empty;  // Unique user login handle for the patient account, initialized to an empty string as default
        public DateTime PreviousDate { get; set; }            // Timestamp recording the creation date of the baseline (earlier) assessment session
        public DateTime CurrentDate { get; set; }             // Timestamp recording the creation date of the target (current) assessment session

        public List<ImprovementDto> Förbättringar { get; set; } = new();  // Collection of questionnaire items demonstrating positive score progression or recovery over time
        public List<ImprovementDto> Försämringar { get; set; } = new();  // Collection of questionnaire items demonstrating symptom regression or negative score shifts over time
        public List<ImprovementDto> Flaggade { get; set; } = new();      // Collection of questionnaire items flagged by clinical staff for mandatory follow-up discussion
        public List<ImprovementDto> Hoppade { get; set; } = new();       // Collection of questionnaire items bypassed by the patient during evaluation sessions
    }
}

/// <summary>
/// Represents a longitudinal overview data transfer object tracking behavioral and symptom changes 
/// in patient self-assessments over time, formatted specifically for clinical staff review dashboards.
/// </summary>
public class PatientChangeOverviewForStaffDto
{
    public string Username { get; set; } = string.Empty;  // Unique user login handle for the patient account, initialized to an empty string as default
    public DateTime PreviousDate { get; set; }            // Timestamp recording the creation date of the baseline patient assessment session
    public DateTime CurrentDate { get; set; }             // Timestamp recording the creation date of the target patient assessment session
    public List<ImprovementDto> Förbättringar { get; set; } = new();  // Collection of questionnaire items showing improvement trajectories in patient self-scores
    public List<ImprovementDto> Försämringar { get; set; } = new();  // Collection of questionnaire items showing regression trajectories in patient self-scores
    public List<ImprovementDto> Hoppade { get; set; } = new(); // Collection of items bypassed by the patient (staff risk flags are omitted as they are not applicable to pure patient self-evaluations)
}