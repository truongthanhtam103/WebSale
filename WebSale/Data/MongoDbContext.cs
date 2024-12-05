using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WebSale.Models;


namespace WebSale.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["WebSale:ConnectionString"]);
            _database = client.GetDatabase(configuration["WebSale:DatabaseName"]);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("User");
        public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");
    }

}
