namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents a comparative overview of changes in a patient's questionnaire responses over time,
    /// structured for clinical staff analysis to track trajectories (improvements, regressions, and skips).
    /// </summary>
    public class PatientChangeOverviewForStaffDto
    {
        public string Username { get; set; } = string.Empty;
        public DateTime PreviousDate { get; set; }
        public DateTime CurrentDate { get; set; }
        public List<ImprovementDto> Förbättringar { get; set; } = new();
        public List<ImprovementDto> Försämringar { get; set; } = new();
        public List<ImprovementDto> Hoppade { get; set; } = new(); // Risk flags are omitted from patient-side trajectory comparisons as they are exclusive to clinical evaluations
    }
}