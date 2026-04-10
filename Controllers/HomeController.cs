using Microsoft.AspNetCore.Mvc;

namespace TransportationManagement.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            string role = HttpContext.Session.GetString("Role") ?? "";
            if (role == "Admin")
                return RedirectToAction("Dashboard", "Admin");

            return View();
        }
    }
}
