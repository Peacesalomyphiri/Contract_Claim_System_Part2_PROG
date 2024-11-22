using Contract_Claim_System.Validation;
using System.ComponentModel.DataAnnotations;

namespace Contract_Claim_System.ViewModels
{
    public class ClaimsViewModel
    {
        public int ClaimID { get; set; }

        [Required(ErrorMessage = "Lecturer Name is required.")]
        public string LecturerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hourly Rate is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Hourly Rate must be a positive value greater than 0.")]
        public decimal HourlyRate { get; set; }

        [Required(ErrorMessage = "Hours Worked is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Hours Worked must be at least 1.")]
        public int HoursWorked { get; set; }

        [Required(ErrorMessage = "Submission Date is required.")]
        [PastDateValidation(ErrorMessage = "Submission Date cannot be in the future.")]
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Supporting Document is required.")]
        public IFormFile SupportingDocument { get; set; }

        public string SupportingDocumentPath { get; set; } = string.Empty;

        public string SupportingDocumentName { get; set; } = string.Empty;

        public string AdditionalNotes { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        public string RejectionReason { get; set; } = string.Empty;
    }
}
