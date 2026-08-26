using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.DTOs.Assessment
{
    /// <summary>
    /// Represents a detailed data transfer object mapping an individual assessment question line item, 
    /// containing comprehensive response metrics, commentary, and flags for professional or administrative reviews.
    /// </summary>
    public class AssessmentItemDto
    {
        public int ItemID { get; set; } // Unique primary key identifier for the specific assessment line item instance

        public int AssessmentID { get; set; } // Foreign key identifier referencing the parent assessment session container

        public int QuestionID { get; set; } // Foreign key identifier referencing the master template question definition

        public int? PatientAnswer { get; set; } // Numerical rating score submitted by the patient (nullable if unanswered)

        public string? PatientComment { get; set; } // Optional descriptive commentary provided by the patient for context

        public int? StaffAnswer { get; set; } // Numerical rating score evaluated and submitted by clinical staff (nullable if pending)

        public string? StaffComment { get; set; } // Optional clinical commentary or feedback provided by the professional reviewer

        public int Order { get; set; } // Sequence index determining the display presentation order within the wizard questionnaire

        public bool Flag { get; set; } // Risk indicator flag set by healthcare staff to highlight items requiring follow-up discussions

        public bool SkippedByPatient { get; set; } // Tracks whether the question item was intentionally bypassed by the patient
    }
}