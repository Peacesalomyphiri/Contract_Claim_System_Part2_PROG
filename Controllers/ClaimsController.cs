using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq; 
using System.Threading.Tasks;
using Contract_Claim_System.Models;
using Contract_Claim_System.ViewModels;

namespace Contract_Claim_System.Controllers
{
    public class ClaimsController : Controller
    {
        // Static list to hold claims in memory
        private static List<ClaimsViewModel> _claims = new List<ClaimsViewModel>();

        private IActionResult CheckUserRole(string[] validRoles)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (!validRoles.Contains(userRole))
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if the role doesn't match
            }
            return null; // Return null if the user has an expected role
        }

        public IActionResult LecturerDashboard()
        {
            var roleCheck = CheckUserRole(new[] { "Lecturer" });
            if (roleCheck != null)
            {
                return roleCheck; // Redirect if the user is not a Lecturer
            }

            return View(_claims); // Show claims for the lecturer
        }

        public IActionResult CoordinatorDashboard()
        {
            var roleCheck = CheckUserRole(new[] { "Coordinator" });
            if (roleCheck != null)
            {
                return roleCheck; // Redirect if the user is not a Coordinator
            }

            return View(_claims); // Show claims for the coordinator
        }

        public IActionResult AcademicManagerDashboard()
        {
            var roleCheck = CheckUserRole(new[] { "Manager" });
            if (roleCheck != null)
            {
                return roleCheck; // Redirect if the user is not an Academic Manager
            }

            return View(_claims); // Show claims for the academic manager
        }

        // Approve Claim for Coordinator and Manager
        [HttpPost]
        public IActionResult ApproveClaim(int index)
        {
            var roleCheck = CheckUserRole(new[] { "Coordinator", "Manager" });
            if (roleCheck != null)
            {
                return roleCheck; // Redirect if the user is not authorized
            }

            if (index >= 0 && index < _claims.Count)
            {
                _claims[index].Status = "Approved"; // Update the claim status
            }

            // Return the current dashboard view after approval
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Coordinator")
            {
                return View("CoordinatorDashboard", _claims); // Keep the user on the current page for Coordinator
            }
            else
            {
                return View("AcademicManagerDashboard", _claims); // Keep the user on the current page for Academic Manager
            }
        }

        // Reject Claim for Coordinator and Manager
        [HttpPost]
        public IActionResult RejectClaim(int index)
        {
            var roleCheck = CheckUserRole(new[] { "Coordinator", "Manager" });
            if (roleCheck != null)
            {
                return roleCheck; // Redirect if the user is not authorized
            }

            if (index >= 0 && index < _claims.Count)
            {
                _claims[index].Status = "Rejected"; // Update the claim status
            }

            // Return the current dashboard view after rejection
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Coordinator")
            {
                return View("CoordinatorDashboard", _claims); // Keep the user on the current page for Coordinator
            }
            else
            {
                return View("AcademicManagerDashboard", _claims); // Keep the user on the current page for Academic Manager
            }
        }

        // Action method to submit claims
        [HttpPost]
        public async Task<IActionResult> SubmitClaim(ClaimsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadDir); 

                if (model.SupportingDocument != null && model.SupportingDocument.Length > 0)
                {
                    var filePath = Path.Combine(uploadDir, model.SupportingDocument.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.SupportingDocument.CopyToAsync(stream);
                    }

                    // This Calculates total amount depending on the hours Worked as well as the rate
                    model.TotalAmount = model.HourlyRate * model.HoursWorked;

                    // Create a new claim and set its properties
                    var claim = new ClaimsViewModel // Change to use your ViewModel
                    {
                        LecturerName = model.LecturerName,
                        HourlyRate = model.HourlyRate,
                        HoursWorked = model.HoursWorked,
                        SubmissionDate = model.SubmissionDate,
                        SupportingDocumentPath = $"/uploads/{model.SupportingDocument.FileName}", // Save relative path for view
                        SupportingDocumentName = model.SupportingDocument.FileName, // Save document name
                        AdditionalNotes = model.AdditionalNotes,
                        TotalAmount = model.TotalAmount,
                        Status = "Pending" // Initial status
                    };

                    // Add the claim to the static list
                    _claims.Add(claim); // Save the claim to the in-memory list
                }

                return RedirectToAction("LecturerDashboard");
            }

            return View(model);
        }
    }
}
