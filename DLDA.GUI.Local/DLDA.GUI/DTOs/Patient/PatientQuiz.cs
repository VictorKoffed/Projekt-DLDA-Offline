namespace DLDA.GUI.DTOs.Patient
{
    /// <summary>
    /// Represents a data transfer object encapsulating a patient's numerical rating score 
    /// and optional descriptive commentary for a questionnaire item response.
    /// </summary>
    public class PatientAnswerDto
    {
        public int? Answer { get; set; } // Numerical score rating provided by the patient (nullable if left unanswered or bypassed)

        public string? Comment { get; set; } // Optional contextual explanation commentary provided by the patient
    }

    /// <summary>
    /// Represents the submission payload data transfer object used when transmitting 
    /// a patient's answered questionnaire item data to the server backend.
    /// </summary>
    public class SubmitAnswerDto
    {
        public int ItemID { get; set;            // Unique primary key identifier referencing the specific assessment line item being answered

            public int Answer { get; set;            // Numerical rating score submitted by the patient, typically scaled within defined questionnaire boundaries (e.g., 0 to 4)

            public string? Comment { get; set;      // Optional descriptive feedback commentary submitted alongside the rating score
        }
    }