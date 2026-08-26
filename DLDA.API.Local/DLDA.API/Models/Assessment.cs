using Microsoft.EntityFrameworkCore;

namespace DLDA.API.Models
{
    /// <summary>
    /// Represents an assessment session entity in the database, containing container metadata,
    /// completion status flags for both patients and staff, and navigation properties to relational user records and line items.
    /// </summary>
    [PrimaryKey(nameof(AssessmentID))]
    public class Assessment
    {
        public int AssessmentID { get; set; }  
        public string? ScaleType { get; set; }
        public DateTime? CreatedAt { get; set; }
        // Timestamp recording when the assessment container or its responses were last modified
        public DateTime? UpdatedAt { get; set; }
        // Indicates whether the patient has completed and finalized their portion of the assessment
        public bool IsComplete { get; set; }
        // Indicates whether healthcare professionals have completed and signed off on their review evaluation
        public bool IsStaffComplete { get; set; }

        // Foreign key and navigation property linking the assessment session to the specific patient user account
        public int UserId { get; set; }
        public User? User { get; set; }

        public ICollection<AssessmentItem> AssessmentItems { get; set; } = new List<AssessmentItem>();
    }
}