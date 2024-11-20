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
        
        // GET: api/products
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
        
        // GET: api/products/5
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
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(ProductDto product)
        {
            var newProduct = await _productFactory.CreateProductAsync(product);

            _repository.Add(newProduct);
            await _repository.SaveChanges();

            return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, newProduct);
        }

        // PUT: api/products/5
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

        // DELETE: api/products/5
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
        /// 
        /// The search can also check descriptions for exact matches, or greater than or less than
        /// 
        /// The attribute can also refer to StockQuantity or productID
        /// </summary>
        /// <param name="Query">String we search for in the description or name, empty query accepts everything (if you only care about attributes)</param>
        /// <param name="Attributes"> A list containing Key=Value pairs</param>
        /// <param name="FuzzyLevel">Higher levels let more results through (WARNING HIGHER LEVEL MAKES THE SEARCH SLOWER)</param>
        /// <param name="IgnoreCase">Consider different cases same</param>
        /// <param name="IgnoreDuplicates">Disregard duplicate letters in misspelling (common mistake for Dyslexic people)</param>
        /// <param name="IgnoreLength">Use a constant penalty for different length words, instead of a cost per letter length difference</param>
        /// <param name="IgnoreCommonTypos">Disregard very common typos, such as swapped b and p or swapped 0 and O</param>
        /// <param name="Name">Search name for query</param>
        /// <param name="Description">Also search description. This is MUCH slower than Name, since descriptions tend to be longer</param>
        /// <returns></returns>
        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts(
            [FromQuery] List<string> Attributes,
            [FromQuery] string Query="",
            [FromQuery] FuzzyText.FuzzyComparer.Level FuzzyLevel=FuzzyText.FuzzyComparer.Level.Strict,
            [FromQuery] bool IgnoreCase=true,
            [FromQuery] bool IgnoreDuplicates=false,
            [FromQuery] bool IgnoreLength=false,
            [FromQuery] bool IgnoreCommonTypos=false,
            [FromQuery] bool Name=true,//Search name for the string
            [FromQuery] bool Description=false//also search description WARNING SLOWER THAN NAME!
            )
        {
            try
            {
                var products = await  _service.GetProductByFuzzySearch(Query,
                    Attributes,
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
                    return Ok(products.Select(_productFactory.CreateProductDto).ToList());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
