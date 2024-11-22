using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contract_Claim_System.Models
{
    [Table("users")] // Explicitly map to "users" table in the database
    public class User
    {
        [Key] // Marks UserId as the primary key
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = string.Empty; // Examples: Lecturer, Coordinator, Manager
    }
}
