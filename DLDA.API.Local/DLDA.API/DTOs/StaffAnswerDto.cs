namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents the data transfer payload submitted when a healthcare professional provides or updates
    /// their evaluation score, clinical commentary, and risk flags for a specific questionnaire item.
    /// </summary>
    public class StaffAnswerDto
    {
        public int? Answer { get; set; }
        public string? Comment { get; set; }
        public bool? Flag { get; set; } // 👈 Required to support clinical risk flagging and attention highlights in professional reviews
    }
}