using Contract_Claim_System.Models;
namespace Contract_Claim_System.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }  // e.g., Lecturer, Coordinator, Manager
    }
}

