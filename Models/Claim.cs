namespace Contract_Claim_System.Models
{
    public class Claim
    {
        public int ClaimID { get; set; }  // Unique claim identifier
        public string LecturerName { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; } = 0m;
        public int HoursWorked { get; set; } = 0;
        public DateTime SubmissionDate { get; set; }
        public string SupportingDocumentPath { get; set; } = string.Empty;
        public string SupportingDocumentName { get; set; } = string.Empty;
        public string AdditionalNotes { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Default status

        // New Property for Rejection Reason
        public string RejectionReason { get; set; } = string.Empty; // Reason for rejection when status is "Rejected"

        // Static criteria for automation
        public static int MaxHoursWorked { get; } = 40;
        public static decimal MaxHourlyRate { get; } = 300m;

        // List of validation errors
        public List<string> ValidationErrors { get; } = new List<string>();

        // Method to validate claim
        public bool IsValidClaim()
        {
            ValidationErrors.Clear();
            if (HoursWorked > MaxHoursWorked)
                ValidationErrors.Add($"Hours worked ({HoursWorked}) exceeds the maximum allowed ({MaxHoursWorked}).");
            if (HourlyRate > MaxHourlyRate)
                ValidationErrors.Add($"Hourly rate ({HourlyRate}) exceeds the maximum allowed ({MaxHourlyRate}).");
            if (string.IsNullOrEmpty(SupportingDocumentPath))
                ValidationErrors.Add("Supporting document is required.");

            return !ValidationErrors.Any();
        }

        // Method to calculate total amount
        public void CalculateTotalAmount()
        {
            TotalAmount = HourlyRate * HoursWorked;
        }

        // Optional constructor for easy initialization
           public Claim(int claimID, string lecturerName, decimal totalAmount, decimal hourlyRate, int hoursWorked, DateTime submissionDate, string supportingDocumentName, string status, string additionalNotes)
    {
        ClaimID = claimID;
        LecturerName = lecturerName;
        TotalAmount = totalAmount;
        HourlyRate = hourlyRate;
        HoursWorked = hoursWorked;
        SubmissionDate = submissionDate;
        SupportingDocumentName = supportingDocumentName;
        Status = status;
        AdditionalNotes = additionalNotes;
    }
    }
}
