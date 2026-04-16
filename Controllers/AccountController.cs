using Microsoft.AspNetCore.Mvc;
using TransportationManagement.Services;
using TransportationManagement.ViewModels;

namespace TransportationManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Username") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _accountService.Authenticate(model.Username, model.Password);
                if (user != null)
                {
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role);
                    HttpContext.Session.SetInt32("UserId", user.Id);

                    return user.Role == "Admin"
                        ? RedirectToAction("Dashboard", "Admin")
                        : RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid email or password.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_accountService.IsUsernameTaken(model.Username))
                {
                    ModelState.AddModelError("Username", "This email is already registered.");
                    return View(model);
                }
                _accountService.CreateAccount(model);
                TempData["Success"] = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}