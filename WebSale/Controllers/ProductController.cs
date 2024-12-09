using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WebSale.Models;

namespace WebSale.Controllers
{
    public class ProductController : Controller
    {
        private readonly IMongoCollection<Product> _products;

        public ProductController(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["WebSale:ConnectionString"]);
            var database = client.GetDatabase(configuration["WebSale:DatabaseName"]);
            _products = database.GetCollection<Product>("Products");
        }

        [HttpGet]
        public IActionResult Index(string category, int page = 1, int pageSize = 9)
        {
            // Lấy danh sách các loại hàng hóa
            var categories = _products.Distinct<string>("Category", Builders<Product>.Filter.Empty).ToList();

            // Lọc sản phẩm theo danh mục và quantity > 0
            var filter = Builders<Product>.Filter.Gt(p => p.Quantity, 0); // Lọc các sản phẩm có quantity > 0
            if (!string.IsNullOrEmpty(category))
            {
                filter = Builders<Product>.Filter.And(
                    filter,
                    Builders<Product>.Filter.Eq(p => p.Category, category)
                );
            }

            var totalProducts = _products.CountDocuments(filter); // Tổng số sản phẩm theo bộ lọc

            // Phân trang
            var products = _products
                .Find(filter)
                .Skip((page - 1) * pageSize) // Bỏ qua sản phẩm của trang trước
                .Limit(pageSize) // Lấy số sản phẩm của trang hiện tại
                .ToList();

            var viewModel = new ProductIndexViewModel
            {
                Categories = categories,
                Products = products,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize),
                SelectedCategory = category
            };

            return View("Index", viewModel);
        }

        // Giao diện quản lý sản phẩm chỉ dành cho admin
        [HttpGet]
        public IActionResult ManageProducts()
        {
            // Kiểm tra quyền Admin
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            if (!isAdmin)
            {
                return RedirectToAction("Index"); // Người dùng thường không có quyền
            }

            // Lấy danh sách sản phẩm cho admin
            var products = _products.Find(_ => true).ToList();
            return View(products); // Hiển thị ManageProducts.cshtml
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            // Chỉ admin mới được thêm sản phẩm
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            if (!isAdmin)
            {
                return RedirectToAction("Index");
            }

            return View(); // Hiển thị form AddProduct
        }

        [HttpPost]
        public IActionResult AddProduct([FromForm] Product product, IFormFile? Avatar)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { message = "Dữ liệu sản phẩm không hợp lệ.", errors });
            }

            // Nếu không có mô tả, đặt giá trị mặc định
            product.Description ??= "Không có mô tả.";

            // Nếu không có avatar, đặt avatar mặc định
            if (Avatar != null)
            {
                var filePath = Path.Combine("wwwroot/images/products", $"{Guid.NewGuid()}{Path.GetExtension(Avatar.FileName)}");
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    Avatar.CopyTo(stream);
                }
                product.Avatar = $"/images/products/{Path.GetFileName(filePath)}";
            }
            else
            {
                product.Avatar = "/images/AvatarDefault/default.png";
            }

            _products.InsertOne(product);

            return Ok(new { message = "Thêm sản phẩm thành công." });
        }

        [HttpPut]
        public IActionResult UpdateProduct([FromForm] Product product, IFormFile Avatar)
        {
            if (product == null || string.IsNullOrEmpty(product.Id))
            {
                return BadRequest(new { message = "Dữ liệu sản phẩm không hợp lệ." });
            }

            // Kiểm tra và gán avatar mặc định nếu chưa có
            if (string.IsNullOrEmpty(product.Avatar))
            {
                product.Avatar = "/images/AvatarDefault/default-avatar.png"; // Đường dẫn ảnh mặc định
            }

            if (Avatar != null)
            {
                var filePath = Path.Combine("wwwroot/images/products", $"{Guid.NewGuid()}{Path.GetExtension(Avatar.FileName)}");
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    Avatar.CopyTo(stream);
                }
                product.Avatar = $"/images/products/{Path.GetFileName(filePath)}";
            }

            var filter = Builders<Product>.Filter.Eq(p => p.Id, product.Id);
            _products.ReplaceOne(filter, product);

            return Ok(new { message = "Cập nhật sản phẩm thành công." });
        }

        [HttpGet]
        public IActionResult GetAllProducts()
        {
            var products = _products.Find(_ => true).ToList();

            // Thêm avatar mặc định nếu không có
            foreach (var product in products)
            {
                if (string.IsNullOrEmpty(product.Avatar))
                {
                    product.Avatar = "/images/AvatarDefault/default-avatar.png";
                }
            }

            return Json(products);
        }

        [HttpDelete]
        public IActionResult DeleteProduct(string id)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            _products.DeleteOne(filter);
            return Ok(new { message = "Xóa sản phẩm thành công." });
        }

        [HttpGet]
        public IActionResult GetProductById(string id)
        {
            var product = _products.Find(p => p.Id == id).FirstOrDefault();
            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm." });
            }
            return Json(product);
        }
    }
}
