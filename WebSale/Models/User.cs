using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebSale.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsLocked { get; set; } = false;
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Cart { get; set; } = new List<string>();
    }

}
