namespace Contract_Claim_System.Models
{
    public class Claim
    {
        public string LecturerName { get; set; } = string.Empty; 
        public decimal HourlyRate { get; set; }
        public int HoursWorked { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string SupportingDocumentPath { get; set; } = string.Empty; 
        public string SupportingDocumentName { get; set; } = string.Empty; 
        public string AdditionalNotes { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Default status
    }
}
