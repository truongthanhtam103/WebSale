using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebSale.Models
{
    public class ProductIndexViewModel
    {
        public List<string> Categories { get; set; }
        public List<Product> Products { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SelectedCategory { get; set; }
    }
}

