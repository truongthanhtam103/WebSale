using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WebSale.Models;

namespace WebSale.Controllers
{
    public class UserController : Controller
    {
        private readonly IMongoCollection<User> _users;

        public UserController(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["WebSale:ConnectionString"]);
            var database = client.GetDatabase(configuration["WebSale:DatabaseName"]);
            _users = database.GetCollection<User>("User");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password, string confirmPassword, string email, string phoneNumber)
        {
            // Kiểm tra mật khẩu nhập lại
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View();
            }

            // Kiểm tra nếu tên đăng nhập đã tồn tại
            var existingUser = _users.Find(u => u.Username == username).FirstOrDefault();
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Username already exists.");
                return View();
            }

            // Tạo user mới
            var newUser = new User
            {
                Username = username,
                Password = password,
                Email = email,
                PhoneNumber = phoneNumber
            };

            _users.InsertOne(newUser);

            // Chuyển hướng đến trang đăng nhập
            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Kiểm tra tài khoản tồn tại trong cơ sở dữ liệu
            var user = _users.Find(u => u.Username == username && u.Password == password).FirstOrDefault();
            if (user == null)
            {
                // Thêm lỗi vào ModelState để hiển thị trên giao diện
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View();
            }

            // Lưu thông tin đăng nhập vào Session
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());

            // Chuyển hướng đến trang sản phẩm
            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ManageUsers()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            if (!isAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            var users = _users.Find(_ => true).ToList(); // Lấy danh sách người dùng từ MongoDB
            return View(users);
        }

        [HttpGet]
        public IActionResult UpdateAccountInfo()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login");

            var user = _users.Find(u => u.Username == username).FirstOrDefault();
            if (user == null) return RedirectToAction("Login");

            return View(user);
        }

        [HttpPost]
        public IActionResult UpdateAccountInfo(User updatedUser)
        {
            if (!ModelState.IsValid)
            {
                return View(updatedUser);
            }

            var filter = Builders<User>.Filter.Eq(u => u.Id, updatedUser.Id);
            _users.ReplaceOne(filter, updatedUser);

            return RedirectToAction("Index", "Home");
        }

    }
}
