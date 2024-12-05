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

        //public IActionResult Index(string category = null)
        //{
        //    // Nếu có category, lọc sản phẩm theo category
        //    var products = string.IsNullOrEmpty(category)
        //        ? _products.Find(_ => true).ToList()
        //        : _products.Find(p => p.Category == category).ToList();

        //    // Lấy danh sách các loại hàng hóa duy nhất
        //    var categories = _products.AsQueryable()
        //        .Select(p => p.Category)
        //        .Distinct()
        //        .ToList();

        //    // Tạo ViewModel để truyền cả sản phẩm và loại hàng hóa
        //    var viewModel = new ProductIndexViewModel
        //    {
        //        Products = products,
        //        Categories = categories
        //    };

        //    return View(viewModel);
        //}

        [HttpGet]
        public IActionResult Index(string category)
        {
            // Giao diện danh sách sản phẩm thông thường
            var categories = _products.Distinct<string>("Category", Builders<Product>.Filter.Empty).ToList();
            var products = string.IsNullOrEmpty(category)
                ? _products.Find(_ => true).ToList()
                : _products.Find(p => p.Category == category).ToList();

            var viewModel = new ProductIndexViewModel
            {
                Categories = categories,
                Products = products
            };

            return View("Index", viewModel); // Hiển thị Index.cshtml
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
                return BadRequest(new { message = "Invalid product data.", errors });
            }

            // Thêm sản phẩm vào cơ sở dữ liệu
            _products.InsertOne(product);

            return Ok(new { message = "Product added successfully." });
        }

        [HttpGet]
        public IActionResult GetAllProducts()
        {
            var products = _products.Find(_ => true).ToList();
            return Json(products); // Trả về danh sách sản phẩm
        }

        //[HttpGet]
        //public IActionResult EditProduct(string id)
        //{
        //    var product = _products.Find(p => p.Id == id).FirstOrDefault();
        //    return View(product);
        //}

        //[HttpPost]
        //public IActionResult EditProduct(Product product)
        //{
        //    var filter = Builders<Product>.Filter.Eq(p => p.Id, product.Id);
        //    _products.ReplaceOne(filter, product);
        //    return RedirectToAction("Index");
        //}

        [HttpDelete]
        public IActionResult DeleteProduct(string id)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            _products.DeleteOne(filter);
            return Ok(new { message = "Product deleted successfully." });
        }

        [HttpGet]
        public IActionResult GetProductById(string id)
        {
            var product = _products.Find(p => p.Id == id).FirstOrDefault();
            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }
            return Json(product);
        }

        [HttpPut]
        public IActionResult UpdateProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return BadRequest(new { message = "Invalid product data.", errors });
            }

            var filter = Builders<Product>.Filter.Eq(p => p.Id, product.Id);
            _products.ReplaceOne(filter, product);
            return Ok(new { message = "Product updated successfully." });
        }


    }
}
