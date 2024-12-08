using Microsoft.AspNetCore.Mvc;
using OnlineShop.Models.Db;
namespace OnlineShop.Controllers
{
    public class ProductsController1 : Controller
    {
        private readonly OnlineShopContext _context;
        public ProductsController1(OnlineShopContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            List<Product> products = _context.Products.OrderByDescending(x => x.Id).ToList();
            return View(products);
        }
    }
}
