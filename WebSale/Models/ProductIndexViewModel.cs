using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebSale.Models
{
    public class ProductIndexViewModel
    {
        public IEnumerable<Product> Products { get; set; } // Danh sách sản phẩm
        public IEnumerable<string> Categories { get; set; } // Danh sách loại hàng hóa
    }
}
