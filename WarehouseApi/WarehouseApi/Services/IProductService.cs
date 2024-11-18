using WarehouseApi.Models;

namespace WarehouseApi.Services
{
    public interface IProductService
    {

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
        public Task<IEnumerable<Product>> GetProductByFuzzySearch(
            string query,
            FuzzyText.FuzzyComparer.Level FuzzyLevel=FuzzyText.FuzzyComparer.Level.Strict,
            bool IgnoreCase=true,
            bool IgnoreDuplicates=false,
            bool IgnoreLength=false,
            bool IgnoreCommonTypos=false,
            bool Name=true,//Search name and category for the string
            bool Category=false,
            bool Description=false//also search description WARNING SLOW SLOW SLOW!
            );
    }
}
