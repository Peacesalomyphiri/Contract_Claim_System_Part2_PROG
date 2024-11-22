using System;
using System.ComponentModel.DataAnnotations;

namespace Contract_Claim_System.Validation
{
    public class PastDateValidationAttribute : ValidationAttribute
    {
        // Override the IsValid method to perform custom validation
        public override bool IsValid(object value)
        {
            // Check if the value is a valid DateTime
            if (value is DateTime dateTime)
            {
                // Check if the date is in the future
                if (dateTime > DateTime.Now)
                {
                    return false; // Invalid if the date is in the future
                }
                return true; // Valid if the date is in the past or present
            }
            return false; // Invalid if the value is not a DateTime
        }
    }
}
