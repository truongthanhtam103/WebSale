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

            // Lọc sản phẩm theo danh mục (nếu có)
            var filter = string.IsNullOrEmpty(category) ? Builders<Product>.Filter.Empty : Builders<Product>.Filter.Eq(p => p.Category, category);
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
        public IActionResult AddProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                Console.WriteLine("Invalid data:", string.Join(", ", errors)); // Log lỗi
                return BadRequest(new { message = "Dữ liệu sản phẩm không hợp lệ.", errors });
            }

            Console.WriteLine("Product to add:", product); // Log dữ liệu sản phẩm
            _products.InsertOne(product);

            return Ok(new { message = "Thêm sản phẩm thành công." });
        }

        [HttpGet]
        public IActionResult GetAllProducts()
        {
            var products = _products.Find(_ => true).ToList();
            return Json(products); // Trả về danh sách sản phẩm
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

        [HttpPut]
        public IActionResult UpdateProduct([FromBody] Product product)
        {
            if (product == null || string.IsNullOrEmpty(product.Id))
            {
                return BadRequest(new { message = "Dữ liệu sản phẩm không hợp lệ." });
            }

            var filter = Builders<Product>.Filter.Eq(p => p.Id, product.Id);
            var result = _products.ReplaceOne(filter, product);

            if (result.ModifiedCount > 0)
            {
                return Ok(new { message = "Chỉnh sửa sản phẩm thành công." });
            }

            return NotFound(new { message = "Không tìm thấy sản phẩm để chỉnh sửa." });
        }

    }
}
