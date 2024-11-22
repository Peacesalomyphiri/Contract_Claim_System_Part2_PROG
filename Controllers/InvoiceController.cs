using Microsoft.AspNetCore.Mvc;

namespace Contract_Claim_System.Controllers
{
    public class InvoiceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
