using System.Threading.Tasks;
using Dipay.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDBConfig>(builder.Configuration.GetSection("MongoDBConfig"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBConfig>>().Value;
    return new MongoClient(settings.ConnectionString);
});

// Register the database service
builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<MongoDBConfig>>().Value;
    return client.GetDatabase(settings.DatabaseName);
});

// Register MVC services
builder.Services.AddControllers();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();
