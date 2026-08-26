namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents a detailed row-by-row comparison matrix mapping discrepancies between
    /// a patient's self-assessment and a healthcare professional's evaluation for clinical review.
    /// </summary>
    public class StaffComparisonRowDto
    {
        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        // 🧑 Patient
        public int? PatientAnswer { get; set; }
        public string? PatientComment { get; set; }

        // 👩‍⚕️ Staff
        public int? StaffAnswer { get; set; }
        public string? StaffComment { get; set; }

        // 🟡 Classification state categorizing perspective alignment (e.g., match, mild-diff, strong-diff, skipped)
        public string Classification { get; set; } = string.Empty;

        // ⛔ Tracks whether the specific question item was intentionally skipped by the patient
        public bool SkippedByPatient { get; set; }

        // 🚩 Indicates items flagged by clinical staff for follow-up discussions or critical attention
        public bool IsFlagged { get; set; }

        // Contextual metadata recording creation timestamp and patient account identifier
        public DateTime CreatedAt { get; set; }
        public string Username { get; set; } = string.Empty;

    }

    /// <summary>
    /// Represents an aggregate longitudinal change overview for staff analysis, categorizing
    /// clinical assessment trajectories across improvements, regressions, risk flags, and skipped items.
    /// </summary>
    public class StaffChangeOverviewDto
    {
        public string Username { get; set; } = string.Empty;
        public DateTime PreviousDate { get; set; }
        public DateTime CurrentDate { get; set; }
        public List<ImprovementDto> Förbättringar { get; set; } = new();
        public List<ImprovementDto> Försämringar { get; set; } = new();
        public List<ImprovementDto> Flaggade { get; set; } = new();
        public List<ImprovementDto> Hoppade { get; set; } = new();
    }
}