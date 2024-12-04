using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseApi.Data_Access;
using WarehouseApi.Models;
using WarehouseApi.Repositories;
using Microsoft.EntityFrameworkCore.InMemory;

namespace WarehouseTests
{
    public class GenericRepositoryTests
    {
        private readonly IRepository<Product> _repository;
        private readonly Mock<DbSet<Product>> _mockSet;
        private readonly Mock<IWarehouseContext> _mockContext;

        public GenericRepositoryTests()
        {
            _mockContext = new Mock<IWarehouseContext>();
            _mockSet = new Mock<DbSet<Product>>();
            _mockContext.Setup(c => c.Set<Product>()).Returns(_mockSet.Object);
            _repository = new GenericEfCoreRepository<Product>(_mockContext.Object);
        }

        // Example Unit Test: Get method for repository
        [Fact]
        public async Task Get_ReturnsAsyncById()
        {
            // Arrange
            _mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(new Product { Id = 1, Name = "test1" });

            // Act
            var result = await _repository.Get(1);

            // Assert
            Assert.NotNull(result);  // Ensure the result is not null
            Assert.Equal(1, result.Id);  // Verify the product's ID
            Assert.Equal("test1", result.Name);  // Verify the product's name
        }
    }
}
