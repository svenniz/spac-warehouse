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
        /// <param name="Category"></param>
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
            bool Category=false,
            bool Description=false//also search description WARNING SLOW SLOW SLOW!
            )
        {
            //Empty query, or nowhere to look returns empty result
            if (query.Length == 0 || (!Name && !Category && !Description))
                return new List<Product>();

            //For now, just return anything which matches (maybe it would be better to order them by the qualityt of the match)
            var myQuery = context.Products.Where(item =>
            //Short circuit logic in C# means that the Equals or Contains functions are only called if a match has not already been found, and we care about Name, Description or Category
            //Check Category first, as it is likely the shortest word
         //   (Category && fuzzyComparer.Equals(item.Category,query,FuzzyLevel,(IgnoreCase ? FuzzyText.FuzzyComparer.Options.IgnoreCase : FuzzyText.FuzzyComparer.Options.None) | (IgnoreCommonTypos ? FuzzyText.FuzzyComparer.Options.IgnoreCommonTypos: FuzzyText.FuzzyComparer.Options.None) | (IgnoreDuplicates ? FuzzyText.FuzzyComparer.Options.IgnoreDuplicates : FuzzyText.FuzzyComparer.Options.None) | (IgnoreLength ? FuzzyText.FuzzyComparer.Options.IgnoreExtraLength : FuzzyText.FuzzyComparer.Options.None) ))||
            (Name && fuzzyComparer.Equals(item.Name,query,FuzzyLevel,(IgnoreCase ? FuzzyText.FuzzyComparer.Options.IgnoreCase : FuzzyText.FuzzyComparer.Options.None) | (IgnoreCommonTypos ? FuzzyText.FuzzyComparer.Options.IgnoreCommonTypos: FuzzyText.FuzzyComparer.Options.None) | (IgnoreDuplicates ? FuzzyText.FuzzyComparer.Options.IgnoreDuplicates : FuzzyText.FuzzyComparer.Options.None) | (IgnoreLength ? FuzzyText.FuzzyComparer.Options.IgnoreExtraLength : FuzzyText.FuzzyComparer.Options.None) ))||
            //This is the most expensive operation BY FAR, as Descriptions tend to be long
            (Description && fuzzyComparer.Contains(item.Description,query,FuzzyLevel,(IgnoreCase ? FuzzyText.FuzzyComparer.Options.IgnoreCase : FuzzyText.FuzzyComparer.Options.None) | (IgnoreCommonTypos ? FuzzyText.FuzzyComparer.Options.IgnoreCommonTypos: FuzzyText.FuzzyComparer.Options.None) | (IgnoreDuplicates ? FuzzyText.FuzzyComparer.Options.IgnoreDuplicates : FuzzyText.FuzzyComparer.Options.None) | (IgnoreLength ? FuzzyText.FuzzyComparer.Options.IgnoreExtraLength : FuzzyText.FuzzyComparer.Options.None) ))
            
            );

            return await myQuery.ToListAsync();
        }
    }
}
