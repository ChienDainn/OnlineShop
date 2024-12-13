using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Models.Db;
using OnlineShop.Models.ViewModels;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

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

            user.Email = user.Email?.Trim();
            user.Password = user.Password?.Trim();
            user.FullName = user.FullName?.Trim();

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
        [HttpPost]
        public IActionResult Login(LoginViewModel user)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            // Find the user in the database
            var foundUser = _context.Users.FirstOrDefault(x => x.Email == user.Email.Trim() && x.Password==user.Password.Trim());
            //------------------
            if (foundUser == null)
            {
                ModelState.AddModelError("Email", "Email or Password is not valid !");
                return View(user);
            }

            // Create claims for the authenticated user
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, foundUser.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, foundUser.FullName));
            claims.Add(new Claim(ClaimTypes.Email, foundUser.Email));

            // Add role-based claim
            if (foundUser.IsAdmin == true)
            {
                claims.Add(new Claim(ClaimTypes.Role, "admin"));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "user"));
            }

            // Additional code for signing in or returning response can go here...
            // Create an identity based on the claims
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Create a principal based on the identity
            var principal = new ClaimsPrincipal(identity);

            // Sign in the user with the created principal
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirect to the home page or another page
            return Redirect("/");

        }
        
        [Authorize]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }


    }
}
