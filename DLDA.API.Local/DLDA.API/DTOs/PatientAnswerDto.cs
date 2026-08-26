namespace DLDA.API.DTOs
{
    /// <summary>
    /// Represents the data transfer payload submitted when a patient provides or updates
    /// their score response and optional descriptive commentary for a questionnaire item.
    /// </summary>
    public class PatientAnswerDto
    {
        public int? Answer { get; set; }
        public string? Comment { get; set; }
    }
}