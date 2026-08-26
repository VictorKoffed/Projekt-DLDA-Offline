namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents the data transfer payload utilized when a patient or user chooses
    /// to intentionally skip a questionnaire item during the assessment wizard flow.
    /// </summary>
    public class SkipQuestionDto
    {
        public int ItemID { get; set; }            // Identifies the specific assessment line item to be flagged as skipped
    }
}