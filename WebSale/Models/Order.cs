using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace WebSale.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Username { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime OrderDate { get; set; }
    }

    public class PurchaseViewModel
    {
        public Product Product { get; set; }
        public User User { get; set; }
        public int Quantity { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class OrderRequest
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string PaymentMethod { get; set; }
    }
}
