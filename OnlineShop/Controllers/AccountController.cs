using Microsoft.AspNetCore.Mvc;
using OnlineShop.Models.Db;
using System.Text.RegularExpressions;

namespace OnlineShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly OnlineShopContext _context;
        public AccountController(OnlineShopContext context)
        {
            _context = context;
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(User user)
        {
            user.RegisterDate = DateTime.Now;
            user.IsAdmin = false;

            user.Email = user.Email.Trim();
            user.Password = user.Password.Trim();
            user.FullName = user.FullName.Trim();

            user.RecoveryCode = 0;

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            // Valid Email Checking
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(user.Email);

            if (!match.Success)
            {
                ModelState.AddModelError("Email", "Email is not valid");
                return View(user);
            }
            // Duplicate Email Checking
            var prevUser = _context.Users.Any(x => x.Email == user.Email);

            if (prevUser)
            {
                ModelState.AddModelError("Email", "Email is used");
                return View(user);
            }

            // Add user to database
            _context.Users.Add(user);
            _context.SaveChanges();

            // Redirect to login page
            return RedirectToAction("login");

        }
        public IActionResult Login()
        {
            return View();
        }
    }
}
