namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents an aggregated statistical summary of a single patient assessment,
    /// providing distribution metrics, severity categories, and top areas of concern for graphical visualization.
    /// </summary>
    public class PatientSingleSummaryDto
    {
        public int AssessmentId { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime CreatedAt { get; set; }
        public int WithoutProblem { get; set; }
        public int MinorIssues { get; set; }
        public int Skipped { get; set; }
        public Dictionary<int, int> AnswerDistribution { get; set; } = new();
        public List<string> Top5ProblematicQuestions { get; set; } = new();
        public List<PatientAnswerStatsDto> Answers { get; set; } = new();
    }
}