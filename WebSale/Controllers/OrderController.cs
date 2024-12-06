using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WebSale.Models;

namespace WebSale.Controllers
{
    public class OrderController : Controller
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Product> _products;
        private readonly IMongoCollection<Order> _orders;

        public OrderController(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["WebSale:ConnectionString"]);
            var database = client.GetDatabase(configuration["WebSale:DatabaseName"]);
            _users = database.GetCollection<User>("User");
            _products = database.GetCollection<Product>("Products");
            _orders = database.GetCollection<Order>("Orders");
        }

        [HttpGet]
        public IActionResult PurchaseProduct(string productId)
        {
            // Lấy thông tin user từ Session
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "User");
            }

            // Lấy thông tin user từ database
            var user = _users.Find(u => u.Username == username).FirstOrDefault();
            if (user == null || user.IsAdmin) // Nếu không tìm thấy hoặc tài khoản là admin
            {
                return Unauthorized("Người dùng không hợp lệ.");
            }

            // Lấy thông tin sản phẩm
            var product = _products.Find(p => p.Id == productId).FirstOrDefault();
            if (product == null)
            {
                return NotFound("Sản phẩm không tồn tại.");
            }

            // Tạo model view để hiển thị sản phẩm và thông tin người dùng
            var viewModel = new PurchaseViewModel
            {
                Product = product,
                User = user,
                Quantity = 1, // Mặc định số lượng là 1
                PaymentMethod = "Trả tiền mặt" // Phương thức thanh toán mặc định
            };

            return View("PurchaseProduct", viewModel);
        }

        [HttpPost]
        public IActionResult PlaceOrder([FromBody] OrderRequest orderRequest)
        {
            // Retrieve the username from the session
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập để mua hàng." });
            }

            // Find the user
            var user = _users.Find(u => u.Username == username).FirstOrDefault();
            if (user == null)
            {
                return Unauthorized(new { message = "Người dùng không hợp lệ." });
            }

            // Find the product
            var product = _products.Find(p => p.Id == orderRequest.ProductId).FirstOrDefault();
            if (product == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại." });
            }

            // Check if the quantity being ordered is greater than available stock
            if (orderRequest.Quantity > product.Quantity)
            {
                return BadRequest(new
                {
                    message = $"Số lượng sản phẩm hiện tại chỉ còn {product.Quantity}. Đơn hàng đã được điều chỉnh theo số lượng có sẵn.",
                    adjustedQuantity = product.Quantity
                });
            }

            // Update the product stock
            product.Quantity -= orderRequest.Quantity;
            var updateResult = _products.ReplaceOne(p => p.Id == product.Id, product);
            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return StatusCode(500, new { message = "Không thể cập nhật số lượng sản phẩm. Vui lòng thử lại sau." });
            }

            // Create the order
            var order = new Order
            {
                ProductId = orderRequest.ProductId,
                ProductName = orderRequest.ProductName,
                Quantity = orderRequest.Quantity,
                TotalPrice = orderRequest.Quantity * orderRequest.ProductPrice,
                ShippingAddress = orderRequest.ShippingAddress,
                PhoneNumber = orderRequest.PhoneNumber,
                PaymentMethod = orderRequest.PaymentMethod,
                OrderDate = DateTime.Now,
                Username = username
            };

            _orders.InsertOne(order); // MongoDB will automatically generate the Id

            return Ok(new { message = "Đặt hàng thành công!", orderId = order.Id });
        }

        [HttpGet]
        public IActionResult History()
        {
            // Lấy thông tin user hiện tại từ session
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "User"); // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
            }

            // Lấy danh sách đơn hàng của user
            var orders = _orders.Find(o => o.Username == username).ToList();

            // Truyền danh sách đơn hàng đến view
            return View("PurchaseHistory", orders); // Trả về View "PurchaseHistory"
        }

    }
}
