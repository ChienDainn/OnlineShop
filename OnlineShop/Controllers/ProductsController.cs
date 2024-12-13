using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models.Db;
using System.Text.RegularExpressions;
namespace OnlineShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly OnlineShopContext _context;
        public ProductsController(OnlineShopContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            List<Product> products = _context.Products.OrderByDescending(x => x.Id).ToList();
            return View(products);
        }

        public IActionResult SearchProducts(string SearchText)
        {
            var products = _context.Products
    .Where(x => EF.Functions.Like(x.Title, "%" + SearchText + "%") || EF.Functions.Like(x.Tags, "%" + SearchText + "%"))
    .OrderBy(x => x.Title)
    .ToList();

            return View("Index", products);
        }

        public IActionResult ProductDetails(int id)
        {
            Product? product = _context.Products.FirstOrDefault(x => x.Id == id);
            //-------
            if (product == null)
            {
                return NotFound();
            }
            ViewData["gallery"] = _context.ProductGaleries.Where(x => x.ProductId == id).ToList();
            //-------
            ViewData["NewProducts"] = _context.Products.Where(x => x.Id != id).Take(6).OrderByDescending(x => x.Id).ToList();

            //-------
            ViewData["comments"] = _context.Comments.Where(x => x.ProductId == id).Take(6).OrderByDescending(x => x.Id).ToList();
            //---------
            return View(product);

        }
        [HttpPost]
        public IActionResult SubmitComment(string name, string email, string comment, int productId)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) &&
                !string.IsNullOrEmpty(comment) && productId != 0)
            {
                Regex regex = new Regex(@"^[\w\.-]+@([\w-]+\.)+[\w-]{2,3}$");
                Match match = regex.Match(email);

                if (!match.Success)
                {
                    TempData["ErrorMessage"] = "Email is not valid";
                    return Redirect("/Products/ProductDetails/" + productId);
                }

                Comment newComment = new Comment
                {
                    Name = name,
                    Email = email,
                    CommentText = comment,
                    ProductId = productId,
                    CreateDate = DateTime.Now
                };

                _context.Comments.Add(newComment);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Your comment was submitted successfully.";
                return Redirect("/Products/ProductDetails/" + productId);
            }
            else
            {
                TempData["ErrorMessage"] = "Please complete your information.";
                return Redirect("/Products/ProductDetails/" + productId);
            }
        }

    }
}
