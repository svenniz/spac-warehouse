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

            var attrKeys = new List<string>();

            foreach (var key in keys)
            {
                if (attrKeys.Contains(key.Name))
                {
                    continue;
                }

                attrKeys.Add(key.Name);
            }


            return attrKeys;
        }

        // GET: api/ProductAttribute/Value
        [HttpGet("Value")]
        public async Task<ActionResult<IEnumerable<string>>> GetProductAttributeValues()
        {
            var values = await _context.ProductAttributeValues.ToListAsync();

            var attrValues = new List<string>();

            foreach (var value in values)
            {
                if (attrValues.Contains(value.Value))
                {
                    continue;
                }

                attrValues.Add(value.Value);
            }

            return attrValues;
        }
    }
}
