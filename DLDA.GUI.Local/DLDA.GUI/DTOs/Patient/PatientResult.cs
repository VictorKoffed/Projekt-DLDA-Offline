namespace DLDA.GUI.DTOs.Patient
{
    /// <summary>
    /// Represents a data transfer object encapsulating statistical summaries and categorized 
    /// severity distributions for an individual completed patient assessment session.
    /// </summary>
    public class PatientSingleSummaryDto
    {
        public int AssessmentId { get; set; } // Unique primary key identifier referencing the assessment session

        public int TotalQuestions { get; set; } // Total count of questions evaluated within the assessment scope

        public DateTime CreatedAt { get; set; } // Timestamp recording when the assessment session was initially created

        public int WithoutProblem { get; set; } // Count of questionnaire items where responses indicate normal baseline status without problems

        public int MinorIssues { get; set; } // Count of questionnaire items indicating mild symptoms or minor issues requiring attention

        public int Skipped { get; set; } // Total count of questionnaire items intentionally bypassed by the patient

        public Dictionary<int, int> AnswerDistribution { get; set; } = new(); // Statistical frequency distribution mapping rating scores to their occurrence counts across the questionnaire items

        public List<string> Top5ProblematicQuestions { get; set; } = new(); // Highlighted list of the top five questions yielding the highest severity scores or problems for prioritized clinical review

        public List<PatientAnswerStatsDto> Answers { get; set; } = new(); // Comprehensive collection of detailed statistical metrics for each individual item response
    }
}