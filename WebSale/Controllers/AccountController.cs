using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WebSale.Models;

namespace WebSale.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Product> _products;

        public AccountController(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["WebSale:ConnectionString"]);
            var database = client.GetDatabase(configuration["WebSale:DatabaseName"]);
            _users = database.GetCollection<User>("Users");
            _products = database.GetCollection<Product>("Products");
        }

        [HttpGet]
        public IActionResult AccountSetting()
        {
            // Kiểm tra session đăng nhập
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                // Nếu chưa đăng nhập, chuyển về trang đăng nhập
                return RedirectToAction("Login", "User");
            }

            // Lấy thông tin tài khoản từ database
            var user = _users.Find(u => u.Username == username).FirstOrDefault();
            if (user == null)
            {
                // Nếu tài khoản không tồn tại, cũng chuyển về trang đăng nhập
                return RedirectToAction("Login", "User");
            }

            // Truyền thông tin người dùng sang view
            return View(user);
        }

        [HttpPost]
        public IActionResult UpdateAccount(string email, string phoneNumber)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "User");

            var filter = Builders<User>.Filter.Eq(u => u.Username, username);
            var update = Builders<User>.Update
                .Set(u => u.Email, email)
                .Set(u => u.PhoneNumber, phoneNumber);

            _users.UpdateOne(filter, update);
            return RedirectToAction("AccountSetting");
        }

        [HttpGet]
        public IActionResult LockUser()
        {
            // Kiểm tra quyền admin trước khi cho phép truy cập
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            if (!isAdmin)
            {
                return RedirectToAction("Index", "Home"); // Redirect về trang chính nếu không phải admin
            }

            // Lấy danh sách tất cả người dùng
            var users = _users.Find(_ => true).ToList();

            return View(users); // Trả về view LockUser.cshtml
        }

        [HttpPost]
        public IActionResult UnlockUser(string userId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Set(u => u.IsLocked, false);

            _users.UpdateOne(filter, update);
            return RedirectToAction("AccountSetting");
        }

        [HttpGet]
        public IActionResult Cart()
        {
            // Giả sử bạn đã có một collection để lưu giỏ hàng
            // Trả về các sản phẩm trong giỏ hàng của người dùng
            return View();
        }

        [HttpGet]
        public IActionResult PurchaseHistory()
        {
            // Trả về danh sách sản phẩm đã mua
            return View();
        }
    }
}
