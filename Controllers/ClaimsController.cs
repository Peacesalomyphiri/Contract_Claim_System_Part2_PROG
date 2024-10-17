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
        private static int _claimCounter = 1; // Class-level static variable
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // File size limit (5 MB)
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
        // Allowed file types
        private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".xlsx" };

        private IActionResult CheckUserRole(string[] validRoles)
        {
            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");
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
                Console.WriteLine("Role check failed."); // Logging
                return roleCheck; // Redirect if the user is not a Lecturer
            }
            // Retrieve success message from TempData
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            Console.WriteLine("Success message: " + ViewBag.SuccessMessage); // Logging
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
            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");
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
            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");
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
                // File handling logic
                if (model.SupportingDocument != null && model.SupportingDocument.Length > 0)
                {
                    // Check file size
                    if (model.SupportingDocument.Length > MaxFileSize)
                    {
                        ModelState.AddModelError("SupportingDocument", "File size exceeds 5 MB limit.");
                        return View(model);
                    }

                    var fileExtension = Path.GetExtension(model.SupportingDocument.FileName);
                    // Check file type
                    if (!AllowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("SupportingDocument", "Invalid file type. Allowed types: .pdf, .docx, .xlsx");
                        return View(model);
                    }

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", model.SupportingDocument.FileName);

                    // Save the file to the server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.SupportingDocument.CopyToAsync(stream);
                    }
                }

                // Create a new claim
                var claim = new ClaimsViewModel
                {
                    ClaimID = _claimCounter++, // Ensure this property exists in ClaimsViewModel
                    LecturerName = _httpContextAccessor.HttpContext.Session.GetString("Username") ?? "Unknown Lecturer",
                    HourlyRate = model.HourlyRate,
                    HoursWorked = model.HoursWorked,
                    SubmissionDate = model.SubmissionDate,
                    SupportingDocumentPath = $"/uploads/{model.SupportingDocument.FileName}",
                    SupportingDocumentName = model.SupportingDocument.FileName,
                    AdditionalNotes = model.AdditionalNotes,
                    TotalAmount = model.HourlyRate * model.HoursWorked, // Calculate total amount
                    Status = "Pending" // Initial status
                };

                // Add the claim to the static list
                _claims.Add(claim);

                TempData["SuccessMessage"] = "Your claim has been submitted successfully."; // Success message
                return RedirectToAction("LecturerDashboard");
            }

            return View(model);
        }
    }
}
