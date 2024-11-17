using Microsoft.AspNetCore.Http;
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

        //Search products, example url:
        // GET: api/products/Search?query[0].Key=Name&query[0].Value=F16&...
        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProducts([FromQuery] Dictionary<string,string> query)
        {
            //Un-comment if you want the console to show the received query
            Console.WriteLine("Received Query");
            foreach (var q in query)
            {
                Console.WriteLine($"{q.Key} : {q.Value}");
            }
            
            try
            {
                var products = await  _service.GetProductByKeyValuesAsync(query);

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
    }
}
