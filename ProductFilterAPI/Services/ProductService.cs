using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using ProductFilterAPI.Models;

namespace ProductFilterAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProductService> _logger;
        private readonly ICacheService _cacheService;

        public ProductService(
            IHttpClientFactory httpClientFactory, 
            ILogger<ProductService> logger,
            ICacheService cacheService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<ResultResponse> GetFilteredProductsAsync(decimal? minPrice, decimal? maxPrice, string? size, string? highlight)
        {
            var cacheKey = $"FilteredProducts_{minPrice}_{maxPrice}_{size}_{highlight}";
            
            var cachedResult = _cacheService.GetFromCache<ResultResponse>(cacheKey);
            if (cachedResult != null)
            {
                return cachedResult;
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetStringAsync("https://pastebin.com/raw/JucRNpWs");

            _logger.LogInformation("Response from external API: {Response}", response);

            var rootObject = JsonSerializer.Deserialize<APIResponse>(response);
            var products = rootObject?.Products ?? new List<Product>();

            List<Product> filteredProducts = FilterProducts(products, minPrice, maxPrice, size);
            
            var filterObject = CreateFilterObject(products, filteredProducts);
            
            if (!string.IsNullOrEmpty(highlight))
            {
                ApplyHighlighting(filteredProducts, highlight);
            }

            var finalResult = new ResultResponse()
            {
                FilteredProducts = filteredProducts,
                Filter = filterObject
            };

            _cacheService.SetCache(cacheKey, finalResult);

            return finalResult;
        }

        private List<Product> FilterProducts(List<Product> products, decimal? minPrice, decimal? maxPrice, string? size)
        {
            var filteredProducts = products.ToList();

            if (minPrice.HasValue || maxPrice.HasValue || !string.IsNullOrEmpty(size))
            {
                if (minPrice.HasValue)
                    filteredProducts = filteredProducts.Where(p => p.Price >= minPrice.Value).ToList();
                if (maxPrice.HasValue)
                    filteredProducts = filteredProducts.Where(p => p.Price <= maxPrice.Value).ToList();
                if (!string.IsNullOrEmpty(size))
                    filteredProducts = filteredProducts.Where(p => p.Sizes.Contains(size, StringComparer.OrdinalIgnoreCase)).ToList();
            }

            return filteredProducts;
        }

        private Filter CreateFilterObject(List<Product> products, List<Product> filteredProducts)
        {
            return new Filter()
            {
                MinPrice = filteredProducts.Any() ? filteredProducts.Min(p => p.Price) : (decimal?)null,
                MaxPrice = filteredProducts.Any() ? filteredProducts.Max(p => p.Price) : (decimal?)null,
                Sizes = products.SelectMany(p => p.Sizes).Distinct().ToArray(),
                CommonWords = GetCommonWords(products.Select(p => p.Description).ToList())
            };
        }

        private void ApplyHighlighting(List<Product> products, string highlight)
        {
            var highlightWords = highlight.Split(',').Select(h => h.Trim()).ToList();
            foreach (var product in products)
            {
                foreach (var highlightWord in highlightWords)
                {
                    product.Description = product.Description.Replace(
                        highlightWord, 
                        $"<em>{highlightWord}</em>", 
                        StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        private string[] GetCommonWords(List<string> descriptions)
        {
            var allWords = descriptions
                           .Where(d => !string.IsNullOrEmpty(d))
                           .SelectMany(d => d.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                           .GroupBy(w => w)
                           .OrderByDescending(g => g.Count())
                           .Skip(5)
                           .Take(10)
                           .Select(g => g.Key)
                           .ToArray();
            return allWords;
        }

    }
} 