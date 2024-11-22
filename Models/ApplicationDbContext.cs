using Microsoft.EntityFrameworkCore;

namespace Contract_Claim_System.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }  // This will map to the "users" table in the database
    }
}
