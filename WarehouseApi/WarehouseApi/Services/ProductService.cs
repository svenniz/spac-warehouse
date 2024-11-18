using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data_Access;
using WarehouseApi.Models;
using WarehouseApi.Services;

namespace WarehouseApi.Services
{
    public class ProductService : IProductService
    {
        private readonly WarehouseContext context;
        //For Fuzzy text search, use this helper class
        private readonly FuzzyText.FuzzyComparer fuzzyComparer;

        public ProductService(WarehouseContext _context)
        {
            context = _context;
            fuzzyComparer = new FuzzyText.FuzzyComparer("commonTyposDanish.txt");
        }

        /// <summary>
        /// Here the query is some string we look for, fuzzy level tells us how many misspellings we may accept
        /// The options tell us if the search ignores case differences, duplicate letters, length differences between strings, and very common typos 
        /// The boolean flags Name, Category and Description tell us what to search in
        /// 
        /// WARNING, Description search is both SLOW and may return too many results
        /// </summary>
        /// <param name="query"></param>
        /// <param name="FuzzyLevel"></param>
        /// <param name="IgnoreCase"></param>
        /// <param name="IgnoreDuplicates"></param>
        /// <param name="IgnoreLength"></param>
        /// <param name="IgnoreCommonTypos"></param>
        /// <param name="Name"></param>
        /// <param name="Description"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Product>> GetProductByFuzzySearch(
            string query,
            FuzzyText.FuzzyComparer.Level FuzzyLevel=FuzzyText.FuzzyComparer.Level.Strict,
            bool IgnoreCase=true,
            bool IgnoreDuplicates=false,
            bool IgnoreLength=false,
            bool IgnoreCommonTypos=false,
            bool Name=true,//Search name and category for the string
            bool Description=false//also search description WARNING SLOW SLOW SLOW!
            )
        {
            //Empty query, or nowhere to look returns empty result
            if (query.Length == 0 || (!Name && !Description))
                return new List<Product>();

            //custom wrapper for the comparison functions, so I don't have to write the options more than once
            var myEquals= (string? s)=>
            {
                if (s == null)
                    return false;
                return Name && fuzzyComparer.Equals(s,query,FuzzyLevel,(IgnoreCase ? FuzzyText.FuzzyComparer.Options.IgnoreCase : FuzzyText.FuzzyComparer.Options.None) | (IgnoreCommonTypos ? FuzzyText.FuzzyComparer.Options.IgnoreCommonTypos: FuzzyText.FuzzyComparer.Options.None) | (IgnoreDuplicates ? FuzzyText.FuzzyComparer.Options.IgnoreDuplicates : FuzzyText.FuzzyComparer.Options.None) | (IgnoreLength ? FuzzyText.FuzzyComparer.Options.IgnoreExtraLength : FuzzyText.FuzzyComparer.Options.None) );
            };
            var myContains = (string? s)=>
            {
                if (s == null)
                    return false;

                return Name && fuzzyComparer.Contains(s,query,FuzzyLevel,(IgnoreCase ? FuzzyText.FuzzyComparer.Options.IgnoreCase : FuzzyText.FuzzyComparer.Options.None) | (IgnoreCommonTypos ? FuzzyText.FuzzyComparer.Options.IgnoreCommonTypos: FuzzyText.FuzzyComparer.Options.None) | (IgnoreDuplicates ? FuzzyText.FuzzyComparer.Options.IgnoreDuplicates : FuzzyText.FuzzyComparer.Options.None) | (IgnoreLength ? FuzzyText.FuzzyComparer.Options.IgnoreExtraLength : FuzzyText.FuzzyComparer.Options.None) );
            };

            var myQuery = context.Products.AsQueryable();

            //Client side search, this is unfortunately required for custom fuzzy search
            var clientProducts = await context.Products.ToListAsync();

            //Short circuit logic means the equals or contains functions only get called when they are needed
            return clientProducts.Where(
                item => (Name && myEquals(item.Name))||
                        (Description && myContains(item.Description))
                ).ToList();
        }
    }
}
