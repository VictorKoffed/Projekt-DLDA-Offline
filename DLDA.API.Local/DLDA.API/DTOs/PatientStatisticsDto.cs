namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents statistical data container for a patient assessment, holding sequence timestamps
    /// and collection items of individual question statistics.
    /// </summary>
    public class PatientStatisticsDto
    {
        public int AssessmentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PatientAnswerStatsDto> Answers { get; set; } = new();
    }

    /// <summary>
    /// Represents statistical metrics for a single question response item within a patient statistics report.
    /// </summary>
    public class PatientAnswerStatsDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int? Answer { get; set; }
    }

    /// <summary>
    /// Represents a comparative overview tracking changes in patient responses between two assessment timelines,
    /// providing computed aggregate shifts and category breakdowns.
    /// </summary>
    public class PatientChangeOverviewDto
    {
        public List<ImprovementApiDto> Förbättringar { get; set; } = new();

        public DateTime PreviousDate { get; set; }
        public DateTime CurrentDate { get; set; }

        // 🔄 Automatic computation: calculates the net decrease in skipped questions between sessions
        public int FärreHoppadeFrågor =>
            Förbättringar.Count(f => f.SkippedPrevious) - Förbättringar.Count(f => f.SkippedCurrent);

        // 🔤 Extracts distinct categories associated with recorded improvements for analytical groupings
        public List<string> FörbättradeKategorier =>
            Förbättringar
                .Where(f => !string.IsNullOrWhiteSpace(f.Category))
                .Select(f => f.Category)
                .Distinct()
                .OrderBy(k => k)
                .ToList();
    }

    /// <summary>
    /// Represents delta metrics for an individual question comparing previous and current evaluation states.
    /// </summary>
    public class ImprovementDto
    {
        public string? Question { get; set; } = string.Empty;
        public int Previous { get; set; }
        public int Current { get; set; }
        public int Change => Previous - Current;
        public string? Category { get; set; } = string.Empty;
        public int QuestionId { get; set; }

        // 🔍 Tracks skip state flags across temporal evaluation comparisons
        public bool SkippedPrevious { get; set; }
        public bool SkippedCurrent { get; set; }
    }

    /// <summary>
    /// Represents an API-optimized data transfer object for tracking score differences and progress shifts between assessments.
    /// </summary>
    public class ImprovementApiDto
    {
        public string Question { get; set; } = string.Empty;
        public int Previous { get; set; }
        public int Current { get; set; }
        public int Change => Previous - Current;
        public string Category { get; set; } = string.Empty;
        public bool SkippedPrevious { get; set; }
        public bool SkippedCurrent { get; set; }
        public int QuestionID { get; set; }
    }
}