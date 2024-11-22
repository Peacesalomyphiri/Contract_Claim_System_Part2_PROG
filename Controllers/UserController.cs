using Microsoft.AspNetCore.Mvc;
using Contract_Claim_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Contract_Claim_System.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public UserController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Displays the list of users
        public IActionResult Index()
        {
            var users = _dbContext.Users.ToList(); // Fetch all users from the database

            if (users == null || users.Count == 0)
            {
                // Log or handle the case where no users are found
                Console.WriteLine("No users found in the database.");
                return View("NoUsersFound"); // Optionally return a view that indicates no users are found
            }

            // Return the users list to the view
            return View(users);
        }


        // Action to add a new user
        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddUser(User user)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Users.Add(user);
                _dbContext.SaveChanges();  // Save the new user to the database
                return RedirectToAction("Index");
            }
            return View(user);
        }
    }
}
