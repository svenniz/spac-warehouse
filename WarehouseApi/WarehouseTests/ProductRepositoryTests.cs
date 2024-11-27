using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using WarehouseApi.Models;
using WarehouseApi.Data_Access;


namespace WarehouseTests
{
    
    public class ProductRepositoryTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ProductRepositoryTests(WebApplicationFactory<Program> factory)
        {
            // Set up the WebApplicationFactory with in-memory database
            _factory = factory;

            // Create an HTTP client
            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real database configuration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<WarehouseContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Register the in-memory database for testing
                    services.AddDbContext<WarehouseContext>(options =>
                        options.UseInMemoryDatabase("TestDatabase"));

                    // Optionally, you can seed test data here if needed
                    using var scope = services.BuildServiceProvider().CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseContext>();
                    dbContext.Database.EnsureCreated();
                    SeedTestData(dbContext);
                });
            }).CreateClient();
        }
        [Fact]
        public async Task GetProducts_ReturnsExpectedProducts()
        {
            // Act
            var response = await _client.GetAsync("/api/products");
            Console.WriteLine($"Request URI: {response.RequestMessage?.RequestUri}");
            Console.WriteLine($"Status Code: {response.StatusCode}");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<List<Product>>(jsonResponse);

            // Assert
            Assert.NotNull(products);
            Assert.Equal(2, products.Count);
            Assert.Equal("Product 1", products[0].Name);
            Assert.Equal("1", products[0].ProductNumber);
            Assert.Equal("test 1", products[0].Description);
            Assert.Equal(1, products[0].StockQuantity);

        }

        private void SeedTestData(WarehouseContext context)
        {
            context.Products.AddRange(
                new Product { Name = "Product 1", ProductNumber = "1", Description = "test 1", StockQuantity = 1 },
                new Product { Name = "Product 2", ProductNumber = "2", Description = "test 2", StockQuantity = 1 }
            );
            context.SaveChanges();
        }
    }
}
