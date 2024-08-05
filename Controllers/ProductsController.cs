using Dipay.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Dipay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMongoCollection<Product> _products;

        public ProductsController(IMongoDatabase database)
        {
            _products = database.GetCollection<Product>("products");
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var products = await _products.Find(product => true).ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest(new { Message = "Invalid ID format" });

            var product = await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            // Id is generated automatically to ensure uniqueness
            product.Id = ObjectId.GenerateNewId().ToString();

            await _products.InsertOneAsync(product);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] Product updatedProduct)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest(new { Message = "Invalid ID format" });

            updatedProduct.Id = id;

            var result = await _products.ReplaceOneAsync(p => p.Id == id, updatedProduct);

            if (result.MatchedCount == 0)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest(new { Message = "Invalid ID format" });

            var result = await _products.DeleteOneAsync(p => p.Id == id);

            if (result.DeletedCount == 0)
                return NotFound();

            return NoContent();
        }

        [HttpGet("count-by-category")]
        public async Task<IActionResult> GetCountByCategory([FromQuery] string? category)
        {
            var pipeline = new List<BsonDocument>();

            if (!string.IsNullOrEmpty(category))
            {
                pipeline.Add(new BsonDocument("$match", new BsonDocument("Category", category)));
            }

            pipeline.Add(
                new BsonDocument(
                    "$group",
                    new BsonDocument
                    {
                        { "_id", "$Category" },
                        { "productCount", new BsonDocument("$sum", 1) }
                    }
                )
            );

            var result = await _products.Aggregate<BsonDocument>(pipeline).ToListAsync();
            if (result.Count == 0)
            {
                return NotFound();
            }

            var formattedResult = result
                .Select(doc => new
                {
                    Category = doc["_id"].AsString,
                    ProductCount = doc["productCount"].ToInt32()
                })
                .ToList();

            return Ok(formattedResult);
        }

        [HttpGet("average-price-by-category")]
        public async Task<IActionResult> GetAveragePriceByCategory([FromQuery] string? category)
        {
            var pipeline = new List<BsonDocument>();

            if (!string.IsNullOrEmpty(category))
            {
                pipeline.Add(new BsonDocument("$match", new BsonDocument("Category", category)));
            }

            pipeline.Add(
                new BsonDocument(
                    "$group",
                    new BsonDocument
                    {
                        { "_id", "$Category" },
                        { "averagePrice", new BsonDocument("$avg", "$Price") }
                    }
                )
            );

            var result = await _products.Aggregate<BsonDocument>(pipeline).ToListAsync();

            if (result.Count == 0)
            {
                return NotFound();
            }

            var formattedResult = result
                .Select(doc => new
                {
                    Category = doc["_id"].AsString,
                    AveragePrice = doc["averagePrice"].ToDouble()
                })
                .ToList();

            return Ok(formattedResult);
        }

        [HttpGet("total-value-by-category")]
        public async Task<IActionResult> GetTotalValueByCategory([FromQuery] string? category)
        {
            var pipeline = new List<BsonDocument>();

            if (!string.IsNullOrEmpty(category))
            {
                pipeline.Add(new BsonDocument("$match", new BsonDocument("Category", category)));
            }

            pipeline.Add(
                new BsonDocument(
                    "$group",
                    new BsonDocument
                    {
                        { "_id", "$Category" },
                        {
                            "totalValue",
                            new BsonDocument(
                                "$sum",
                                new BsonDocument("$multiply", new BsonArray { "$Price", "$Stock" })
                            )
                        }
                    }
                )
            );

            var result = await _products.Aggregate<BsonDocument>(pipeline).ToListAsync();

            if (result.Count == 0)
            {
                return NotFound();
            }

            var formattedResult = result
                .Select(doc => new
                {
                    Category = doc["_id"].AsString,
                    TotalValue = doc["totalValue"].ToDouble()
                })
                .ToList();

            return Ok(formattedResult);
        }
    }
}
