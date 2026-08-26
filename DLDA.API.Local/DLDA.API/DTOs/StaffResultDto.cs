namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents an aggregate result container detailing clinical assessment findings,
    /// patient identification data, and structured professional evaluation rows.
    /// </summary>
    public class StaffResultDto
    {
        public int AssessmentId { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? ScaleType { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<StaffResultRowDto> Questions { get; set; } = new();
    }

    /// <summary>
    /// Represents an individual row within a staff result matrix, comparing patient self-assessment
    /// scores against professional evaluations and calculating discrepancy deltas.
    /// </summary>
    public class StaffResultRowDto
    {
        public int ItemID { get; set; }
        public int Order { get; set; } // Represents the sequence index position within the rendered table list
        public string QuestionText { get; set; } = string.Empty;

        public int? PatientAnswer { get; set; }
        public int? StaffAnswer { get; set; }
        public string? PatientComment { get; set; }
        public string? StaffComment { get; set; }
        public bool Flag { get; set; }
        public bool SkippedByPatient { get; set; }

        public int Difference =>
            (PatientAnswer.HasValue && StaffAnswer.HasValue)
                ? Math.Abs(PatientAnswer.Value - StaffAnswer.Value)
                : -1;
    }

    /// <summary>
    /// Represents a comprehensive overview DTO for professional result review dashboards,
    /// tracking clinical sign-off status alongside detailed comparative question rows.
    /// </summary>
    public class StaffResultOverviewDto
    {
        public int AssessmentId { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsStaffComplete { get; set; } 
        public List<StaffResultRowDto> Questions { get; set; } = new();

    }
}