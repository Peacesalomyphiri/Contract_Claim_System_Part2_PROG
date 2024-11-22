using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Linq;
using System.Threading.Tasks;
using Contract_Claim_System.Models;
using Contract_Claim_System.ViewModels;

namespace Contract_Claim_System.Controllers
{
    public class ClaimsController : Controller
    {
        private static List<ClaimsViewModel> _claims = new List<ClaimsViewModel>();
        private static int _claimCounter = 1; // Unique claim ID counter
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB file size limit
        private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".xlsx" };

        private const int MaxHoursWorked = 40; // Automation criteria: Maximum hours
        private const decimal MaxHourlyRate = 100m; // Automation criteria: Maximum hourly rate

        private IActionResult CheckUserRole(string[] validRoles)
        {
            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");
            if (!validRoles.Contains(userRole))
            {
                return RedirectToAction("Login", "Account");
            }
            return null;
        }

        public IActionResult LecturerDashboard()
        {
            var roleCheck = CheckUserRole(new[] { "Lecturer" });
            if (roleCheck != null) return roleCheck;

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(_claims);
        }

        public IActionResult CoordinatorDashboard()
        {
            var roleCheck = CheckUserRole(new[] { "Coordinator" });
            if (roleCheck != null) return roleCheck;

            // Log or debug output to check claim statuses
            foreach (var claim in _claims)
            {
                System.Diagnostics.Debug.WriteLine($"Claim ID: {claim.ClaimID}, Status: {claim.Status}");
            }

            return View(_claims.Where(c => c.Status == "Pending Manager Approval").ToList());

        }

        public IActionResult AcademicManagerDashboard()
        {
            var roleCheck = CheckUserRole(new[] { "Manager" });
            if (roleCheck != null) return roleCheck;

            return View(_claims.Where(c => c.Status == "Pending Manager Approval").ToList());
        }

        [HttpPost]
        public IActionResult ApproveClaim(int index)
        {
            var roleCheck = CheckUserRole(new[] { "Coordinator", "Manager" });
            if (roleCheck != null) return roleCheck;

            if (index >= 0 && index < _claims.Count)
            {
                _claims[index].Status = "Approved";
            }

            return RedirectToAction("CoordinatorDashboard");
        }

        [HttpPost]
        public IActionResult RejectClaim(int index, string rejectionReason)
        {
            var roleCheck = CheckUserRole(new[] { "Coordinator", "Manager" });
            if (roleCheck != null) return roleCheck;

            if (index >= 0 && index < _claims.Count)
            {
                var claim = _claims[index];
                claim.Status = "Rejected";
                claim.RejectionReason = rejectionReason; // Store the rejection reason
            }

            return RedirectToAction("CoordinatorDashboard");
        }

        [HttpPost]
        public async Task<IActionResult> SubmitClaim(ClaimsViewModel model)
        {
            if (ModelState.IsValid)
            {
                // File validation
                if (model.SupportingDocument != null && model.SupportingDocument.Length > 0)
                {
                    if (model.SupportingDocument.Length > MaxFileSize)
                    {
                        ModelState.AddModelError("SupportingDocument", "File size exceeds 5 MB limit.");
                        return View(model);
                    }

                    var fileExtension = Path.GetExtension(model.SupportingDocument.FileName);
                    if (!AllowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("SupportingDocument", "Invalid file type.");
                        return View(model);
                    }

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", model.SupportingDocument.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.SupportingDocument.CopyToAsync(stream);
                    }
                }

                // Claim creation
                var claim = new ClaimsViewModel
                {
                    ClaimID = _claimCounter++,
                    LecturerName = _httpContextAccessor.HttpContext.Session.GetString("Username") ?? "Unknown Lecturer",
                    HourlyRate = model.HourlyRate,
                    HoursWorked = model.HoursWorked,
                    SubmissionDate = model.SubmissionDate,
                    SupportingDocumentPath = $"/uploads/{model.SupportingDocument.FileName}",
                    SupportingDocumentName = model.SupportingDocument.FileName,
                    AdditionalNotes = model.AdditionalNotes,
                    TotalAmount = model.HourlyRate * model.HoursWorked,
                };

                // Automation for claim validation
                if (model.HoursWorked > MaxHoursWorked || model.HourlyRate > MaxHourlyRate)
                {
                    claim.Status = "Pending Manager Approval";
                }
                else if (string.IsNullOrEmpty(claim.SupportingDocumentPath))
                {
                    claim.Status = "Pending Review";
                }
                else
                {
                    claim.Status = "Approved";
                }

                // Add claim to the list
                _claims.Add(claim);
                TempData["SuccessMessage"] = "Claim submitted successfully.";
                return RedirectToAction("LecturerDashboard");
            }

            return View(model);
        }
        public void GenerateClaimReport(List<Claim> claims)
        {
            // Filter approved claims
            var approvedClaims = claims.Where(c => c.Status == "Approved").ToList();

            // Create the directory path for the report
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Reports");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Create the file path for the PDF
            string filePath = Path.Combine(directoryPath, "ApprovedClaimsReport.pdf");

            // Create a document and set up the PDF writer
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                Document document = new Document();
                PdfWriter writer = PdfWriter.GetInstance(document, fs);
                document.Open();

                // Add a title to the document
                document.Add(new Paragraph("Approved Claims Report"));
                document.Add(new Paragraph($"Generated on: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n\n"));

                // Set up the table with the columns
                PdfPTable table = new PdfPTable(9); // 9 columns for each data point
                table.WidthPercentage = 100; // Set table width to fill the page

                // Add table headers
                table.AddCell("ClaimID");
                table.AddCell("Lecturer Name");
                table.AddCell("Hourly Rate");
                table.AddCell("Hours Worked");
                table.AddCell("Submission Date");
                table.AddCell("Supporting Document");
                table.AddCell("Total Amount");
                table.AddCell("Status");
                table.AddCell("Additional Notes");

                // Add rows for each approved claim
                foreach (var claim in approvedClaims)
                {
                    string formattedDate = claim.SubmissionDate.ToString("yyyy-MM-dd"); // Adjust the date format as needed
                    string formattedAmount = claim.TotalAmount.ToString("C"); // Currency formatting (e.g., $100.00)

                    // Check if SupportingDocument has a valid link (assuming it's a file path or URL)
                    string supportingDocLink = claim.SupportingDocumentName;  // Assuming it contains the URL or path to the file

                    // Add claim data to the table
                    table.AddCell(claim.ClaimID.ToString());
                    table.AddCell(claim.LecturerName);
                    table.AddCell(claim.HourlyRate.ToString("F2")); // Display HourlyRate with two decimal places
                    table.AddCell(claim.HoursWorked.ToString("F2")); // Display HoursWorked with two decimal places
                    table.AddCell(formattedDate);

                    // Create a clickable link for the Supporting Document
                    if (Uri.IsWellFormedUriString(supportingDocLink, UriKind.Absolute))
                    {
                        // If it's a valid URL, create a link
                        Font linkFont = new Font(Font.FontFamily.HELVETICA, 12, Font.UNDERLINE, BaseColor.BLUE);
                        Anchor link = new Anchor(supportingDocLink, linkFont);
                        link.Reference = supportingDocLink;  // Set the URL for the link
                        PdfPCell linkCell = new PdfPCell(link);
                        linkCell.Border = Rectangle.NO_BORDER;  // Remove border around the link
                        table.AddCell(linkCell);
                    }
                    else
                    {
                        // If the supporting document is not a valid URL, just display the name or message
                        table.AddCell(supportingDocLink ?? "No document");
                    }

                    table.AddCell(formattedAmount);
                    table.AddCell(claim.Status);
                    table.AddCell(claim.AdditionalNotes);
                }

                // Add the table to the document
                document.Add(table);
                document.Close();
            }

            // Notify that the report has been generated
            System.Diagnostics.Debug.WriteLine($"Report generated at: {filePath}");
        }
        [HttpGet]
public IActionResult GenerateReport()
{
    // Check if the user has permission
    var roleCheck = CheckUserRole(new[] { "Coordinator", "Manager" });
    if (roleCheck != null) return roleCheck;

    // Map the claims into the correct model (Claim)
    var claimList = _claims.Select(c => new Claim(
               c.ClaimID,                // Pass the ClaimID
               c.LecturerName,           // LecturerName
               c.TotalAmount,            // TotalAmount
               c.HourlyRate,             // HourlyRate
               c.HoursWorked,            // HoursWorked (if applicable)
               c.SubmissionDate,         // SubmissionDate
               c.SupportingDocumentName, // SupportingDocument (Name)
               c.Status,                 // Status
               c.AdditionalNotes         // AdditionalNotes
    )).ToList();

    // Generate the report with the mapped claims
    GenerateClaimReport(claimList);

    // Notify user or redirect after generation
    TempData["SuccessMessage"] = "Claim report generated successfully.";
    return RedirectToAction("CoordinatorDashboard");
}

        [HttpGet]
        public IActionResult DownloadReport()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Reports", "ApprovedClaimsReport.pdf");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("The report file does not exist.");
            }

            // Return the file as a downloadable file
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", "ApprovedClaimsReport.pdf");
        }



    }
}
