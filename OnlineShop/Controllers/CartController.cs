using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OnlineShop.Models.Db;
using OnlineShop.Models.ViewModels;

namespace OnlineShop.Controllers
{
    public class CartController : Controller
    {
        private OnlineShopContext _context;
        public CartController(OnlineShopContext context)
        {
            _context= context;
        }
        public IActionResult Index()
        {
            var result = GetProductsInCart();
            return View(result);
        }
        public IActionResult ClearCart()
        {
            Response.Cookies.Delete("Cart");
            return Redirect("/");
        }

        [HttpPost]
        public IActionResult UpdateCart([FromBody] CartViewModel request)
        {
            var product = _context.Products.FirstOrDefault(x => x.Id == request.ProductId);
            if (product == null)
            {
                return NotFound();
            }

            // Retrieve the list of products in the cart using the dedicated function
            var cartItems = GetCartItems();

            var foundProductInCart = cartItems.FirstOrDefault(x => x.ProductId == request.ProductId);
            // If the product is found, it means it is in the cart, and the user intends to change the quantity
            if (foundProductInCart == null)
            {
                var newCartItem = new CartViewModel() { };
                newCartItem.ProductId = request.ProductId;
                newCartItem.Count = request.Count;

                cartItems.Add(newCartItem);
            }
            else
            {
                // If greater than zero, it means the user wants to update the quantity; otherwise, it will be removed
                if (request.Count > 0)
                {
                    foundProductInCart.Count = request.Count;
                }
                else
                {
                    cartItems.Remove(foundProductInCart);
                }
            }

            var json = JsonConvert.SerializeObject(cartItems);

            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddDays(7);
            Response.Cookies.Append("Cart", json, option);

            var result = cartItems.Sum(x => x.Count);

            return Ok (result);

        }
        public List<CartViewModel> GetCartItems()
        {
            List<CartViewModel> cartList = new List<CartViewModel>();

            var prevCartItemsString = Request.Cookies["Cart"];

            // If it's not null, it means the cart is not empty, so we need to convert it to a list of CartViewModel objects.
            // Otherwise, we return an empty cart list.
            if (!string.IsNullOrEmpty(prevCartItemsString))
            {
                cartList = JsonConvert.DeserializeObject<List<CartViewModel>>(prevCartItemsString);
            }

            return cartList;
        }
        public List<ProductCartViewModel> GetProductsInCart()
        {
            var cartItems = GetCartItems();

            // Kiểm tra xem giỏ hàng có rỗng hay không. Nếu rỗng, trả về một partial view với nội dung null.
            if (!cartItems.Any())
                return null;

            // Lấy danh sách các ProductId từ các sản phẩm trong giỏ hàng để chuẩn bị truy vấn.
            var cartItemProductIds = cartItems.Select(x => x.ProductId).ToList();

            // Truy vấn cơ sở dữ liệu để lấy thông tin chi tiết các sản phẩm trong giỏ hàng.
            // Chỉ lấy các sản phẩm có ID nằm trong danh sách cartItemProductIds.
            var products = _context.Products
                .Where(p => cartItemProductIds.Contains(p.Id))
                .ToList();

            // Khởi tạo danh sách để chứa các đối tượng ProductCartViewModel.
            List<ProductCartViewModel> result = new List<ProductCartViewModel>();

            // Duyệt qua từng sản phẩm được lấy từ cơ sở dữ liệu.
            foreach (var item in products)
            {
                // Tạo một đối tượng ProductCartViewModel mới cho từng sản phẩm.
                var newItem = new ProductCartViewModel
                {
                    Id = item.Id, // ID của sản phẩm
                    ImageName = item.ImageName, // Tên file hình ảnh của sản phẩm (dùng để hiển thị)
                    Price = item.Price - (item.Discount ?? 0), // Giá cuối cùng sau khi trừ giảm giá (nếu có)
                    Title = item.Title, // Tên hoặc tiêu đề của sản phẩm
                    Count = cartItems.Single(x => x.ProductId == item.Id).Count, // Số lượng sản phẩm này trong giỏ hàng
                    RowSumPrice = (item.Price - (item.Discount ?? 0)) *
                                  cartItems.Single(x => x.ProductId == item.Id).Count // Tổng giá trị của sản phẩm này (giá * số lượng)
                };

                // Thêm đối tượng vừa tạo vào danh sách kết quả.
                result.Add(newItem);
            }

            // Trả về một partial view và truyền danh sách ProductCartViewModel cho view đó.
            return result;
        }
        public IActionResult SmallCart()
        {
            var result = GetProductsInCart();
            return PartialView(result);
        }

        [Authorize]
        public IActionResult Checkout()
        {
            var order = new Models.Db.Order();

            var shipping = _context.Settings.First().Shipping;
            if (shipping != null)
            {
                order.Shipping = shipping;
            }

            ViewData["Products"] = GetProductsInCart();
            return View(order);
        }

        [Authorize]
        [HttpPost]
        public IActionResult ApplyCouponCode([FromForm] string couponCode)
        {
            var order = new Models.Db.Order();

            var coupon = _context.Coupons.FirstOrDefault(c => c.Code == couponCode);

            if (coupon != null)
            {
                order.CouponCode = coupon.Code;
                order.CouponDiscount = coupon.Discount;
                TempData["message"] = "Coupon applied successfully!"; // Thông báo mã hợp lệ
            }
            else
            {
                ViewData["Products"] = GetProductsInCart();
                TempData["message"] = "Coupon not exist";
                return View("Checkout", order);
            }

            var shipping = _context.Settings.First().Shipping;
            if (shipping != null)
            {
                order.Shipping = shipping;
            }

            ViewData["Products"] = GetProductsInCart();
            return View("Checkout", order);
        }



    }
}
