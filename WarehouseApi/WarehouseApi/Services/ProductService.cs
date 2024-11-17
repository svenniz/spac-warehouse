using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data_Access;
using WarehouseApi.Models;
using WarehouseApi.Services;

namespace WarehouseApi.Services
{
    public class ProductService : IProductService
    {
        private readonly WarehouseContext context;

        public ProductService(WarehouseContext _context)
        {
            context = _context;
        }

        public async Task<IEnumerable<Product>> GetProductByKeyValuesAsync(Dictionary<string, string> query)
        {
            //Empty query, empty result
            if (query.Count() == 0)
                return new List<Product>();

            //First get a reference to all boxes, we will apply each query one by one, gradually shrinking it
            IQueryable<Product> myQuery = context.Products;

            //Each query shrinks the query until it only contains everything we need
            foreach (var q in query)
            {
                //If all properties are STRINGS, we could just use this function to get the property from entityFramework directly:
                //myQuery = myQuery.Where(item => EF.Property<string>(item,q.Key)==q.Value);
                
                //I choose to use a switch instead, that way we will also be able to handle if some fields are integers or floats or whatever

                switch (q.Key.ToLower())
                {
                    case "name":
                        //Case insensitive
                        myQuery = myQuery.Where(item => item.Name.ToLower() == q.Value.ToLower());
                        break;
                    case "category":
                        myQuery = myQuery.Where(item => item.Category.ToLower() == q.Value.ToLower());
                        break;
                    default:
                        throw new ArgumentException($"\"{q.Key}\" is not a searchable category, should be name or category");

                }
                Console.WriteLine($"Item {q.Key}={q.Value}");
            }

            return await myQuery.ToListAsync();
        }
    }
}
