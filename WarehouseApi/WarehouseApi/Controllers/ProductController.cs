﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WarehouseApi.Data_Access;
using WarehouseApi.Dto;
using WarehouseApi.Factories;
using WarehouseApi.Models;
using WarehouseApi.Repositories;
using WarehouseApi.Services;

namespace WarehouseApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductFactory _productFactory;
        private readonly IProductService _service;
        private readonly IProductRepository _repository;
        
        public ProductController(
            IProductFactory productFactory, 
            IProductService service,
            IProductRepository repository)
        {
            _productFactory = productFactory;
            _service = service;
            _repository = repository;
        }


        /// <summary>
        /// Retrieves all products from the repository.
        /// </summary>
        /// <returns>A list of <see cref="ProductDto"/> objects representing all products.</returns>
        /// <response code="200">Returns the list of products.</response>
        /// <response code="500">If an error occurs while retrieving the products.</response>
        /// <response code="404">Not Found. The product with the specified ID does not exist.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
        {
            try
            {
                var products = await _repository.GetAllProductsAsync();
                List<ProductDto> productDtos = products.Select(_productFactory.CreateProductDto).ToList();
                return Ok(productDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        /// <summary>
        /// Retrieves a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to retrieve.</param>
        /// <returns>An ActionResult containing the product with the specified ID.</returns>
        /// <response code="200">Returns the product with the specified ID.</response>
        /// <response code="404">Not Found. The product with the specified ID does not exist.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            try
            {
                var product = await _repository.GetProductAsync(id);

                if (product == null)
                {
                    return NotFound();
                }

                var productDto = _productFactory.CreateProductDto(product);
                return Ok(productDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="product">The product to create.</param>
        /// <returns>An ActionResult containing the created product.</returns>
        /// <response code="201">Created. The product was successfully created.</response>
        /// <response code="400">Bad Request. The product was not created.</response>
        [HttpPost]
        public async Task<ActionResult<ProductDto>> PostProduct(ProductDto product)
        {
            var newProduct = await _productFactory.CreateProductAsync(product);

            _repository.Add(newProduct);
            await _repository.SaveChanges();

            return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, product);
        }

        /// <summary>
        /// Updates a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="product">The updated product.</param>
        /// <returns>An ActionResult containing the updated product.</returns>
        /// <response code="204">No Content. The product was successfully updated.</response>
        /// <response code="400">Bad Request. The product was not updated.</response>
        /// <response code="404">Not Found. The product with the specified ID does not exist.</response>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> PutProduct(int id, ProductDto product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }
            var productToUpdate = await _repository.GetProductAsync(id);
            if (productToUpdate == null)
            {
                return NotFound();
            }

            var modifiedProduct = await _productFactory.CreateProductAsync(product);

            productToUpdate.ProductNumber = product.ProductNumber;
            productToUpdate.Name = product.Name;
            productToUpdate.Description = product.Description;
            productToUpdate.StockQuantity = product.StockQuantity;
            productToUpdate.ProductAttributes = modifiedProduct.ProductAttributes;

            _repository.UpdateProductAsync(productToUpdate);

            try
            {
                await _repository.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_repository.ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>An ActionResult indicating the result of the delete operation.</returns>
        /// <response code="204">No Content. The product was successfully deleted.</response>
        /// <response code="404">Not Found. The product with the specified ID does not exist.</response>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _repository.GetProductAsync(id);
            if(product == null)
            { 
                return NotFound(); 
            }
            _repository.Delete(product);
            await _repository.SaveChanges();
            
            return NoContent();
        }                
                
        /// <summary>
        /// Search function which supports fuzzy search. There is only a single query, which we will search for in the name, category  and description (if the name, category, and description bools are set to do so)
        /// Be warned that searching description is slow
        /// 
        /// The fuzzy level tells us how willing we are to accept misspellings, the Ignore options allow us to accept any case, common typos (like number 0 instead of letter O), dublicate letters (like teling vs telling), or not penalise strings with different length
        /// </summary>
        /// <param name="query"></param>
        /// <param name="FuzzyLevel">Higher levels let more results through (WARNING HIGHER LEVEL MAKES THE SEARCH SLOWER)</param>
        /// <param name="IgnoreCase"></param>
        /// <param name="IgnoreDuplicates"></param>
        /// <param name="IgnoreLength"></param>
        /// <param name="IgnoreCommonTypos"></param>
        /// <param name="Name">Search name for query</param>
        /// <param name="Description">Also search description (Slower than Name)</param>
        /// <returns></returns>
        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProducts(
            [FromQuery]string query,
            [FromQuery]FuzzyText.FuzzyComparer.Level FuzzyLevel=FuzzyText.FuzzyComparer.Level.Strict,
            [FromQuery]bool IgnoreCase=true,
            [FromQuery]bool IgnoreDuplicates=false,
            [FromQuery]bool IgnoreLength=false,
            [FromQuery]bool IgnoreCommonTypos=false,
            [FromQuery]bool Name=true,//Search name for the string
            [FromQuery]bool Description=false//also search description WARNING SLOWER THAN NAME!
            )
        {
            try
            {
                var products = await  _service.GetProductByFuzzySearch(query,
                    FuzzyLevel,
                    IgnoreCase,
                    IgnoreDuplicates,
                    IgnoreLength,
                    IgnoreCommonTypos,
                    Name,
                    Description
                    );

                if (products == null || products.Count()==0)
                
                    return NotFound();
                else
                    return Ok(products);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
