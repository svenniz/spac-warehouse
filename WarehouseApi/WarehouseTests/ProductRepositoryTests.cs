using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseApi.Repositories;
using Moq;
using WarehouseApi.Controllers;
using Microsoft.EntityFrameworkCore;
using WarehouseApi.Models;
using WarehouseApi.Data_Access;
using AutoMapper;

namespace WarehouseTests
{
    public class ProductRepositoryTests
    {

        private readonly ProductRepository _repository;
        private readonly Mock<DbSet<Product>> _mockSet;
        private readonly Mock<WarehouseContext> _mockContext;
        private readonly Mock<IMapper> _mockMap;

        public ProductRepositoryTests()
        {
            _mockContext = new Mock<WarehouseContext>();
            _mockSet = new Mock<DbSet<Product>>();
            _mockMap = new Mock<IMapper>();
            _repository = new ProductRepository(_mockContext.Object, _mockMap.Object);
        }

        [Fact]
        public async Task GetProducts_ReturnsAllProducts()
        {
            // Arrange


            // Act

            // Assert
        }
    }
}
