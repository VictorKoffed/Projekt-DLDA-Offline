using Microsoft.EntityFrameworkCore;

namespace DLDA.API.Models
{
    /// <summary>
    /// Represents an individual questionnaire question instance bound to a specific assessment session,
    /// tracking responses, comments, and flags from both patients and healthcare professionals.
    /// </summary>
    [PrimaryKey(nameof(ItemID))]
    public class AssessmentItem
    {
        // Primary key uniquely identifying this assessment line item instance
        public int ItemID { get; set; }

        // Foreign key mapping this line item back to its parent assessment session container
        public int AssessmentID { get; set; }

        // Foreign key linking to the base question definition catalog entity
        public int QuestionID { get; set; }

        // Patient score response evaluated on a Likert scale (0 = no problem, 4 = very severe problem)
        public int? PatientAnswer { get; set; }

        // Optional descriptive commentary provided by the patient for this specific item
        public string? PatientComment { get; set; }

        // Score rating evaluated and submitted by clinical staff during professional reviews
        public int? StaffAnswer { get; set; }

        // Determines the sorted sequence position index for rendering questions in the wizard workflow
        public int Order { get; set; }

        // Professional clinical commentary recorded by healthcare staff for this item
        public string? StaffComment { get; set; }

        // Timestamp tracking when the response was last created or modified
        public DateTime? AnsweredAt { get; set; } = DateTime.UtcNow;
       
        // Indicator flag denoting whether the patient intentionally bypassed this question item
        public bool SkippedByPatient { get; set; } = false;

        // Risk flag set by clinical staff to highlight items requiring follow-up discussions or special focus
        public bool Flag { get; set; }

        // Relational navigation properties linking to parent assessment and master question entities
        public Assessment? Assessment { get; set; }
        public Question? Question { get; set; }
    }
}