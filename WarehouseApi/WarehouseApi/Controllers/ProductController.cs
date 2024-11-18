using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WarehouseApi.Data_Access;
using WarehouseApi.Dto;
using WarehouseApi.Factories;
using WarehouseApi.Models;
using WarehouseApi.Services;

namespace WarehouseApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly WarehouseContext _context;
        private readonly ProductFactory _productFactory;
        private readonly IProductService _service;
        
        public ProductController(WarehouseContext context, ProductFactory productFactory, IProductService service)
        {
            _context = context;
            _productFactory = productFactory;
            _service = service;
        }
        
        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
        {
            var products = await _context.Products.Include(p => p.ProductAttributes)
        .ThenInclude(pa => pa.ProductAttributeKey)
        .Include(p => p.ProductAttributes)
        .ThenInclude(pa => pa.ProductAttributeValue)
        .ToListAsync();

        List<ProductDto> productDtos = products.Select(p => _productFactory.CreateProductDto(p)).ToList();

        return Ok(productDtos);
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.Include(p => p.ProductAttributes)
        .ThenInclude(pa => pa.ProductAttributeKey)
        .Include(p => p.ProductAttributes)
        .ThenInclude(pa => pa.ProductAttributeValue)
        .FirstOrDefaultAsync(p => p.Id == id);
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
        public async Task<ActionResult<Product>> PostProduct(ProductDto product)
        {
            var newProduct = await _productFactory.CreateProductAsync(product);

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, newProduct);
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductDto product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            var productToUpdate = await _context.Products.Include(p => p.ProductAttributes)
        .ThenInclude(pa => pa.ProductAttributeKey)
        .Include(p => p.ProductAttributes)
        .ThenInclude(pa => pa.ProductAttributeValue)
        .FirstOrDefaultAsync(p => p.Id == id);
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

            _context.Entry(productToUpdate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
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
