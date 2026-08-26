namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents the data transfer payload submitted when a healthcare professional evaluates
    /// and submits an answer for a specific questionnaire item, including score rating, risk flags, and clinical commentary.
    /// </summary>
    public class SubmitStaffAnswerDto
    {
        public int ItemID { get; set; }            // Identifies the specific assessment line item being targeted for the professional evaluation submission
        public int? Answer { get; set; }           // Numerical score rating on the assessment scale (0–4)
        public bool? Flag { get; set; }            // Indicator flag highlighting items requiring special attention or follow-up discussions
        public string? Comment { get; set; }       // Professional clinical commentary elaborating on the evaluation
    }
}