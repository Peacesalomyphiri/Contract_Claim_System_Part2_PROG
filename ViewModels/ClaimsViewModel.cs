using Microsoft.AspNetCore.Http; // Ensure this using directive is included
using System;
using System.ComponentModel.DataAnnotations;

namespace Contract_Claim_System.ViewModels
{
    public class ClaimsViewModel
    {
        [Required(ErrorMessage = "Lecturer Name is required.")]
        public string LecturerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hourly Rate is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Hourly Rate must be a positive value.")]
        public decimal HourlyRate { get; set; }

        [Required(ErrorMessage = "Hours Worked is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Hours Worked must be at least 1.")]
        public int HoursWorked { get; set; }

        [Required(ErrorMessage = "Submission Date is required.")]
        public DateTime SubmissionDate { get; set; }

        [Required(ErrorMessage = "Supporting Document is required.")]
        public IFormFile SupportingDocument { get; set; }

        public string SupportingDocumentPath { get; set; } = string.Empty;

        public string SupportingDocumentName { get; set; } = string.Empty; // Add this property for the document name

        public string AdditionalNotes { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending"; // Default status
    }
}
