using MongoDB.Driver;

namespace Dipay.Models
{
    public class MongoDBConfig
    {
        public required string ConnectionString { get; set; }
        public required string DatabaseName { get; set; }
    }
}
