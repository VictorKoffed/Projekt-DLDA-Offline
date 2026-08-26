using Microsoft.EntityFrameworkCore;

namespace DLDA.API.Models
{
    /// <summary>
    /// Represents the master template definition of a questionnaire item in the system catalog,
    /// storing prompt text, categorical grouping, and activation state for assessment generation.
    /// </summary>
    [PrimaryKey(nameof(QuestionID))]
    public class Question
    {
        // Primary key uniquely identifying the master question definition entry
        public int QuestionID { get; set; }

        // The localized text prompt presented to users during an assessment evaluation
        public string? QuestionText { get; set; } = string.Empty;

        // Categorical classification grouping used for diagnostic filtering and summary reports (e.g., SelfCare, Mobility)
        public string? Category { get; set; } = string.Empty;

        // Activation toggle determining whether the question is included in new active assessment instances (defaults to true)
        public bool IsActive { get; set; } = true;

        // Timestamp recording when the master question definition was originally created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relational navigation property linking this master question definition to active evaluation line item instances
        public ICollection<AssessmentItem> AssessmentItems { get; set; } = new List<AssessmentItem>();
    }
}