using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data_Access;
using WarehouseApi.Dto;
using WarehouseApi.Factories;
using WarehouseApi.Models;

namespace WarehouseApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly WarehouseContext _context;
        private readonly ProductFactory _productFactory;
        public ProductController(WarehouseContext context, ProductFactory productFactory)
        {
            _context = context;
            _productFactory = productFactory;
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
    }
}
