namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents the data transfer payload submitted when a patient answers a specific question
    /// during the assessment wizard flow, including their score and optional commentary.
    /// </summary>
    public class SubmitAnswerDto
    {
        public int ItemID { get; set; }            // Identifies the specific assessment line item being targeted for the answer submission
        public int Answer { get; set; }            // Numerical rating score on the Likert scale (0–4) indicating problem severity
        public string? Comment { get; set; }       // Optional descriptive commentary provided by the patient to elaborate on their response
    }
}