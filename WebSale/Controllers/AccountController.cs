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
            _users = database.GetCollection<User>("User");
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

        [HttpPost]
        [Route("Account/LockUser")]
        public IActionResult LockUser([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "ID người dùng là bắt buộc." });
            }

            // Tìm user và khóa
            var user = _users.Find(u => u.Id == id).FirstOrDefault();
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update.Set(u => u.IsLocked, true);
            var result = _users.UpdateOne(filter, update);

            if (result.ModifiedCount > 0)
            {
                return Ok(new { message = "Người dùng đã khóa thành công." });
            }

            return BadRequest(new { message = "Không khóa được người dùng." });
        }

        [HttpPost]
        [Route("Account/UnlockUser")]
        public IActionResult UnlockUser([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "ID người dùng là bắt buộc." });
            }

            // Tìm user và mở khóa
            var user = _users.Find(u => u.Id == id).FirstOrDefault();
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update.Set(u => u.IsLocked, false);
            var result = _users.UpdateOne(filter, update);

            if (result.ModifiedCount > 0)
            {
                return Ok(new { message = "Người dùng đã mở khóa thành công." });
            }

            return BadRequest(new { message = "Không mở khóa được người dùng." });
        }

        [HttpGet]
        [Route("Account/ManageUsers")]
        public IActionResult ManageUsers()
        {
            var users = _users.Find(_ => true).ToList();
            return View("LockUser", users);
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
