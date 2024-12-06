using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using WebSale.Models;

namespace WebSale.Controllers
{
    public class CartController : Controller
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Product> _products;

        public CartController(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["WebSale:ConnectionString"]);
            var database = client.GetDatabase(configuration["WebSale:DatabaseName"]);
            _users = database.GetCollection<User>("User");
            _products = database.GetCollection<Product>("Products");
        }

        [HttpPost]
        public IActionResult AddToCart([FromBody] CartRequest request)
        {
            if (string.IsNullOrEmpty(request.ProductId))
            {
                return BadRequest(new { message = "Product ID is missing." });
            }

            // Lấy thông tin user từ Session
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                return Unauthorized(new { message = "You need to log in to add items to the cart." });
            }

            // Tìm user trong database
            var user = _users.Find(u => u.Username == username).FirstOrDefault();
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Kiểm tra sản phẩm có tồn tại không
            var product = _products.Find(p => p.Id == request.ProductId).FirstOrDefault();
            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            // Thêm sản phẩm vào giỏ hàng nếu chưa có
            if (!user.Cart.Contains(request.ProductId))
            {
                user.Cart.Add(request.ProductId);
                _users.ReplaceOne(u => u.Id == user.Id, user);
            }

            return Ok(new { message = "Product added to cart successfully." });
        }

        [HttpGet]
        [Route("Cart/ViewCart")] // Route cụ thể
        public IActionResult ViewCart()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                return Unauthorized(new { message = "You need to log in to view the cart." });
            }

            var user = _users.Find(u => u.Username == username).FirstOrDefault();
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var products = _products.Find(p => user.Cart.Contains(p.Id)).ToList();
            return View(products); // Truyền danh sách sản phẩm tới View
        }
        [HttpPost]
        public IActionResult RemoveFromCart([FromBody] CartRequest request)
        {
            if (string.IsNullOrEmpty(request.ProductId))
            {
                return BadRequest(new { message = "Product ID is missing." });
            }

            // Lấy thông tin user từ Session
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                return Unauthorized(new { message = "You need to log in to modify the cart." });
            }

            // Tìm user trong database
            var user = _users.Find(u => u.Username == username).FirstOrDefault();
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Xóa sản phẩm khỏi giỏ hàng
            if (user.Cart.Contains(request.ProductId))
            {
                user.Cart.Remove(request.ProductId);
                _users.ReplaceOne(u => u.Id == user.Id, user);
            }

            return Ok(new { message = "Product removed from cart successfully." });
        }

    }
}
