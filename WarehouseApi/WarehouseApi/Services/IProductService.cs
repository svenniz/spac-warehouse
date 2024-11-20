using WarehouseApi.Dto;
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
        /// <param name="Query">String we search for in the description or name, empty query accepts everything (if you only care about attributes)</param>
        /// <param name="Attributes"> A list containing Key=Value pairs</param>
        /// <param name="FuzzyLevel">Higher levels let more results through (WARNING HIGHER LEVEL MAKES THE SEARCH SLOWER)</param>
        /// <param name="IgnoreCase">Consider different cases same</param>
        /// <param name="IgnoreDuplicates">Disregard duplicate letters in misspelling (common mistake for Dyslexic people)</param>
        /// <param name="IgnoreLength">Use a constant penalty for different length words, instead of a cost per letter length difference</param>
        /// <param name="IgnoreCommonTypos">Disregard very common typos, such as swapped b and p or swapped 0 and O</param>
        /// <param name="Name">Search name for query</param>
        /// <param name="Description">Also search description. This is MUCH slower than Name, since descriptions tend to be longer</param>
        /// <returns></returns>
        public Task<IEnumerable<Product>> GetProductByFuzzySearch(
            string Query,
            List<string> Attributes,
            FuzzyText.FuzzyComparer.Level FuzzyLevel=FuzzyText.FuzzyComparer.Level.Strict,
            bool IgnoreCase=true,
            bool IgnoreDuplicates=false,
            bool IgnoreLength=false,
            bool IgnoreCommonTypos=false,
            bool Name=true,//Search name and category for the string
            bool Description=false//also search description WARNING SLOW SLOW SLOW!
            );
    }
}
