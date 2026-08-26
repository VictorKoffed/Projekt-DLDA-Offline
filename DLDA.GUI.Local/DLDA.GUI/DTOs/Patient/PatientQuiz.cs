namespace DLDA.GUI.DTOs.Patient
{
    /// <summary>
    /// Represents a data transfer object encapsulating a patient's numerical rating score 
    /// and optional descriptive commentary for a questionnaire item response.
    /// </summary>
    public class PatientAnswerDto
    {
        public int? Answer { get; set; }

        public string? Comment { get; set; }
    }

    /// <summary>
    /// Represents the submission payload data transfer object used when transmitting 
    /// a patient's answered questionnaire item data to the server backend.
    /// </summary>
    public class SubmitAnswerDto
    {
        public int ItemID { get; set; }

        public int Answer { get; set; }

        public string? Comment { get; set; }
    }
}