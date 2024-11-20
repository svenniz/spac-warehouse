using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data_Access;
using WarehouseApi.Dto;
using WarehouseApi.Models;
using WarehouseApi.Repositories;

namespace WarehouseApi.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository repository;
        //For Fuzzy text search, use this helper class
        private readonly FuzzyText.FuzzyComparer fuzzyComparer;

        public ProductService(IProductRepository _repository)
        {
            repository = _repository;
            fuzzyComparer = new FuzzyText.FuzzyComparer("commonTyposDanish.txt");
        }

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
        public async Task<IEnumerable<Product>> GetProductByFuzzySearch(
            string Query,
            List<string> Attributes,
            FuzzyText.FuzzyComparer.Level FuzzyLevel = FuzzyText.FuzzyComparer.Level.Strict,
            bool IgnoreCase = true,
            bool IgnoreDuplicates = false,
            bool IgnoreLength = false,
            bool IgnoreCommonTypos = false,
            bool Name = true,//Search name and category for the string
            bool Description = false//also search description WARNING SLOW SLOW SLOW!
            )
        {
            //I we are searching for a non-empty query with nowhere to look, there are no results
            if (Query.Length == 0 && (!Name && !Description))
                return new List<Product>();

            var ProductsQueryable = repository.GetProductWithIncludes();

            //Check all attributes
            foreach (var keyValueAttribute in Attributes)
            {
                string Key;
                string Value;
                if (keyValueAttribute.Contains('='))
                {
                    var splitPair = keyValueAttribute.Split('=', StringSplitOptions.TrimEntries);
                    if (splitPair.Length != 2)
                        throw new Exception($"Could not split {keyValueAttribute} it must cdontain exactly one = > or < symbol");

                    Key = splitPair[0].ToLower();
                    Value = splitPair[1].ToLower();
                    
                    //Reduce the query to anything which has an attribute pair matching what we want
                    ProductsQueryable = ProductsQueryable.Where(
                        //Select all the products, whose attributes have at least one attribute
                        product => product.ProductAttributes.Where(attr =>
                            //With key matching our key
                            attr.ProductAttributeKey.Name.ToLower().Trim() == Key/*We already trimmed this and made it lower*/ &&
                            //And matching the string value (if comparing with = for string values)
                            attr.ProductAttributeValue.Value.ToLower().Trim() == Value
                        ).Any());//If this product has ANY matching pairs, we will take it
                }
                else
                    throw new Exception($"Could not split {keyValueAttribute} it must cdontain exactly one = > or < symbol");
            }
            
            //If we ONLY did an attribute search, stop now
            if (Query.Length == 0)
                return await ProductsQueryable.ToListAsync();

            //custom wrapper for the comparison functions, so I don't have to write the options more than once
            var myEquals = (string? s) =>
            {
                if (s == null)
                    return false;
                return Name && fuzzyComparer.Equals(s, Query, FuzzyLevel, (IgnoreCase ? FuzzyText.FuzzyComparer.Options.IgnoreCase : FuzzyText.FuzzyComparer.Options.None) | (IgnoreCommonTypos ? FuzzyText.FuzzyComparer.Options.IgnoreCommonTypos : FuzzyText.FuzzyComparer.Options.None) | (IgnoreDuplicates ? FuzzyText.FuzzyComparer.Options.IgnoreDuplicates : FuzzyText.FuzzyComparer.Options.None) | (IgnoreLength ? FuzzyText.FuzzyComparer.Options.IgnoreExtraLength : FuzzyText.FuzzyComparer.Options.None));
            };
            var myContains = (string? s) =>
            {
                if (s == null)
                    return false;

                return Name && fuzzyComparer.Contains(s, Query, FuzzyLevel, (IgnoreCase ? FuzzyText.FuzzyComparer.Options.IgnoreCase : FuzzyText.FuzzyComparer.Options.None) | (IgnoreCommonTypos ? FuzzyText.FuzzyComparer.Options.IgnoreCommonTypos : FuzzyText.FuzzyComparer.Options.None) | (IgnoreDuplicates ? FuzzyText.FuzzyComparer.Options.IgnoreDuplicates : FuzzyText.FuzzyComparer.Options.None) | (IgnoreLength ? FuzzyText.FuzzyComparer.Options.IgnoreExtraLength : FuzzyText.FuzzyComparer.Options.None));
            };

            //Client side search, this is unfortunately required for custom fuzzy search
            var clientProducts = await ProductsQueryable.ToListAsync();

            //Short circuit logic means the equals or contains functions only get called when they are needed
            return clientProducts.Where(
                item => (Name && myEquals(item.Name)) ||
                        (Description && myContains(item.Description))
                ).ToList();
        }
    }
}
