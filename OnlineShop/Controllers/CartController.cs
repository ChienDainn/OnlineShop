using Microsoft.AspNetCore.Mvc;
using OnlineShop.Models.Db;

namespace OnlineShop.Controllers
{
    public class CartController : Controller
    {
        private readonly OnlineShopContext _context;
        public CartController(OnlineShopContext context)
        {
            _context= context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
