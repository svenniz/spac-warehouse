using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data_Access;
using System.Linq;

namespace WarehouseApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductAttributeController : ControllerBase
    {
        private readonly WarehouseContext _context;

        public ProductAttributeController(WarehouseContext context)
        {
            _context = context;
        }

        // GET: api/ProductAttribute/Key
        [HttpGet("Key")]
        public async Task<ActionResult<IEnumerable<string>>> GetProductAttributeKeys()
        {
            var keys = await _context.ProductAttributeKeys.ToListAsync();

            var dtos = new List<string>();

            foreach (var key in keys)
            {
                if (dtos.Contains(key.Name))
                {
                    continue;
                }

                dtos.Add(key.Name);
            }


            return dtos;
        }

        // GET: api/ProductAttribute/Value
        [HttpGet("Value")]
        public async Task<ActionResult<IEnumerable<string>>> GetProductAttributeValues()
        {
            var values = await _context.ProductAttributeValues.ToListAsync();

            var dtos = new List<string>();

            foreach (var value in values)
            {
                if (dtos.Contains(value.Value))
                {
                    continue;
                }
                
                dtos.Add(value.Value);
            }

            return dtos;
        }
    }
}
