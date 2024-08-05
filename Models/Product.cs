using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Dipay.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] // Represents Id as ObjectId in MongoDB
        public string? Id { get; set; }
        public required string Name { get; set; }
        public string? Category { get; set; }
        public double Price { get; set; } = 0;
        public int Stock { get; set; } = 0;
    }
}
