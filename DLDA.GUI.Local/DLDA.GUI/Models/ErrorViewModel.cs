namespace DLDA.GUI.Models
{
    /// <summary>
    /// Represents the view model encapsulating diagnostic error context, 
    /// enabling conditional tracking and display of telemetry request identifiers on error pages.
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; } // Unique telemetry trace identifier captured for distributed tracing and server-side log correlation during failure exceptions

        // Evaluates whether a valid request identifier exists to determine if tracing diagnostics should be rendered in the error UI
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}