namespace DLDA.GUI.DTOs.Patient
{
    /// <summary>
    /// Represents a data transfer object encapsulating statistical response metrics 
    /// for a specific patient assessment session.
    /// </summary>
    public class PatientStatistics
    {
        public int AssessmentId { get; set; } // Unique primary key identifier referencing the assessment session

        public DateTime CreatedAt { get; set; } // Timestamp recording when the assessment session was initially created

        public List<PatientAnswerStatsDto> Answers { get; set; } = new(); // Collection of statistical details for each individual question response within the assessment
    }

    /// <summary>
    /// Represents a data transfer object capturing specific statistical metrics 
    /// and text details for a single question response.
    /// </summary>
    public class PatientAnswerStatsDto
    {
        public int QuestionId { get; set; } // Foreign key identifier referencing the master template question definition

        public string QuestionText { get; set; } = string.Empty; // Localized prompt text for the question, initialized to an empty string as a safe fallback default

        public int? Answer { get; set; } // Numerical rating score submitted by the patient for this question item (nullable if unanswered)
    }

    /// <summary>
    /// Represents an overview data transfer object tracking longitudinal progress, 
    /// score shifts, and behavioral changes between two comparative assessment sessions.
    /// </summary>
    public class PatientChangeOverviewDto
    {
        public List<ImprovementDto> Förbättringar { get; set; } = new(); // Collection of granular item-by-item comparison metrics

        public DateTime PreviousDate { get; set; } // Timestamp recording the creation date of the baseline (earlier) assessment session

        public DateTime CurrentDate { get; set; } // Timestamp recording the creation date of the target (current) assessment session

        // Automatically computes the net change in skipped questions to evaluate user engagement trends across sessions
        public int FärreHoppadeFrågor =>
            Förbättringar.Count(f => f.SkippedPrevious) - Förbättringar.Count(f => f.SkippedCurrent);

        // Extracts a distinct, alphabetically sorted list of unique questionnaire categories showing progress
        public List<string> FörbättradeKategorier =>
            Förbättringar
                .Where(f => !string.IsNullOrWhiteSpace(f.Category))
                .Select(f => f.Category)
                .Distinct()
                .OrderBy(k => k)
                .ToList();
    }


    /// <summary>
    /// Represents detailed comparative shift metrics for a single questionnaire item 
    /// evaluated across two distinct assessment sessions.
    /// </summary>
    public class ImprovementDto
    {
        public string? Question { get; set; } = string.Empty; // Localized prompt text for the questionnaire item, initialized to an empty string as default

        public int Previous { get; set; } // Rating score recorded during the baseline (earlier) assessment session

        public int Current { get; set; } // Rating score recorded during the target (current) assessment session

        // Computes the mathematical delta score to quantify symptom progression or recovery between sessions
        public int Change => Previous - Current;

        public string Category { get; set; } = string.Empty; // Classification grouping category for the question item, initialized to an empty string as default

        public int QuestionId { get; set; } // Foreign key identifier referencing the master template question definition

        public bool SkippedPrevious { get; set; } // Tracks whether the question item was intentionally bypassed during the baseline assessment

        public bool SkippedCurrent { get; set; } // Tracks whether the question item was intentionally bypassed during the target assessment
    }

    namespace DLDA.GUI.DTOs.Patient
    {
        /// <summary>
        /// Represents the data transfer object mapped from backend API responses 
        /// detailing comparative improvement metrics for individual questionnaire items over time.
        /// </summary>
        public class ImprovementApiDto
        {
            public string Question { get; set; } = string.Empty; // Localized prompt text for the questionnaire item, initialized to an empty string as default

            public int Previous { get; set; } // Rating score recorded during the baseline assessment session

            public int Current { get; set; } // Rating score recorded during the target assessment session

            // Computes the mathematical delta score to track directional severity changes between sessions
            public int Change => Previous - Current;

            public string Category { get; set; } = string.Empty; // Classification grouping category for the question item, initialized to an empty string as default

            public bool SkippedPrevious { get; set; } // Tracks whether the question item was bypassed during the baseline assessment session

            public bool SkippedCurrent { get; set; } // Tracks whether the question item was bypassed during the target assessment session

            public int QuestionID { get; set; } // Foreign key identifier referencing the master template question definition
        }
    }
}