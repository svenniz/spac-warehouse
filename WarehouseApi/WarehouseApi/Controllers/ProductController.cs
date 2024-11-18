﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WarehouseApi.Data_Access;
using WarehouseApi.Models;
using WarehouseApi.Services;

namespace WarehouseApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly WarehouseContext _context;
        private readonly IProductService _service;
        public ProductController(WarehouseContext context,IProductService service)

        {
            _context = context;
            _service = service;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return product;
        }
            
        
        /// <summary>
        /// Search function which supports fuzzy search. There is only a single query, which we will search for in the name, category  and description (if the name, category, and description bools are set to do so)
        /// Be warned that searching description is slow
        /// 
        /// The fuzzy level tells us how willing we are to accept misspellings, the Ignore options allow us to accept any case, common typos (like number 0 instead of letter O), dublicate letters (like teling vs telling), or not penalise strings with different length
        /// </summary>
        /// <param name="query"></param>
        /// <param name="FuzzyLevel"></param>
        /// <param name="IgnoreCase"></param>
        /// <param name="IgnoreDuplicates"></param>
        /// <param name="IgnoreLength"></param>
        /// <param name="IgnoreCommonTypos"></param>
        /// <param name="Name"></param>
        /// <param name="Category"></param>
        /// <param name="Description"></param>
        /// <returns></returns>
        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProducts(
            [FromQuery]string query,
            [FromQuery]FuzzyText.FuzzyComparer.Level FuzzyLevel=FuzzyText.FuzzyComparer.Level.Strict,
            [FromQuery]bool IgnoreCase=true,
            [FromQuery]bool IgnoreDuplicates=false,
            [FromQuery]bool IgnoreLength=false,
            [FromQuery]bool IgnoreCommonTypos=false,
            [FromQuery]bool Name=true,//Search name and category for the string
            [FromQuery]bool Category=false,
            [FromQuery]bool Description=false//also search description WARNING SLOW!!!
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
                    Category,
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

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new {id = product.Id}, product);
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(int id, Product product)
        {
            if(id != product.Id) 
            { 
                return BadRequest(); 
            }
            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) 
            {
                if(!_context.Products.Any(e=>e.Id == id))
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
            var product = await _context.Products.FindAsync(id);
            if(product == null)
            { 
                return NotFound(); 
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }
}
