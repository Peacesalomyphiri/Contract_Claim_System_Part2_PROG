﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Contract_Claim_System.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string Username, string Password, string Role)
        {
            if (IsValidUser(Username, Password, Role))
            {
                // Store role and username in session 
                HttpContext.Session.SetString("UserRole", Role);
                HttpContext.Session.SetString("Username", Username);  // Store username in session

                // Redirect based on role
                if (Role == "Lecturer")
                    return RedirectToAction("LecturerDashboard", "Claims");
                else if (Role == "Coordinator")
                    return RedirectToAction("CoordinatorDashboard", "Claims");
                else if (Role == "Manager")
                    return RedirectToAction("AcademicManagerDashboard", "Claims");
            }

            // Show error if login failed
            ViewBag.ErrorMessage = "Invalid username, password, or role. Please try again.";
            return View();
        }

        private bool IsValidUser(string username, string password, string role)
        {
            // Dummy user validation logic for demonstration
            return (username == "Peace" && password == "Peace@123" && role == "Lecturer") ||
                   (username == "Yanama" && password == "Yanama@123" && role == "Coordinator") ||
                   (username == "Tarce" && password == "Tarce@123" && role == "Manager");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();

            // Redirect to the login page
            return RedirectToAction("Login");
        }
    }
}
