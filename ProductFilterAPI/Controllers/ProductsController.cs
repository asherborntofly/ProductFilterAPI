using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json.Serialization;
using System.Text.Json;
using ProductFilterAPI.Models;

namespace ProductFilterAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProductsController> _logger;
        private readonly IMemoryCache _cache;

        public ProductsController(IHttpClientFactory httpClientFactory, ILogger<ProductsController> logger, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// Retrieves a list of filtered products based on optional price range and size parameters.
        /// Highlights specified words in product descriptions.
        /// </summary>
        /// <param name="minPrice">Optional minimum price filter.</param>
        /// <param name="maxPrice">Optional maximum price filter.</param>
        /// <param name="size">Optional size filter.</param>
        /// <param name="highlight">Comma-separated list of words to highlight in product descriptions.</param>
        /// <returns>An IActionResult containing the filtered products and their associated filter information.</returns>
        [HttpGet("filter")]
        public async Task<IActionResult> GetFilteredProducts(
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? size,
            [FromQuery] string? highlight)
        {
            var cacheKey = $"FilteredProducts_{minPrice}_{maxPrice}_{size}_{highlight}";
            try
            {
                if (!_cache.TryGetValue(cacheKey, out Result finalResult))
                {

                    var client = _httpClientFactory.CreateClient();
                    var response = await client.GetStringAsync("https://pastebin.com/raw/JucRNpWs");

                    _logger.LogInformation("Response from external API: {Response}", response);

                    var rootObject = JsonSerializer.Deserialize<RootObject>(response);
                    var products = rootObject?.Products ?? new List<Product>();

                    List<Product> filteredProducts = products.ToList();

                    if (minPrice.HasValue || maxPrice.HasValue || !string.IsNullOrEmpty(size))
                    {
                        if (minPrice.HasValue)
                            filteredProducts = filteredProducts.Where(p => p.Price >= minPrice.Value).ToList();
                        if (maxPrice.HasValue)
                            filteredProducts = filteredProducts.Where(p => p.Price <= maxPrice.Value).ToList();
                        if (!string.IsNullOrEmpty(size))
                            filteredProducts = filteredProducts.Where(p => p.Sizes.Contains(size, StringComparer.OrdinalIgnoreCase)).ToList();
                    }

                    Filter filterObject = new Filter()
                    {
                        MinPrice = filteredProducts.Any() ? filteredProducts.Min(p => p.Price) : (decimal?)null,
                        MaxPrice = filteredProducts.Any() ? filteredProducts.Max(p => p.Price) : (decimal?)null,
                        Sizes = products.SelectMany(p => p.Sizes).Distinct().ToArray(),
                        CommonWords = GetCommonWords(products.Select(p => p.Description).ToList())
                    };

                    if (!string.IsNullOrEmpty(highlight))
                    {
                        var highlightWords = highlight.Split(',').Select(h => h.Trim()).ToList();
                        foreach (var product in filteredProducts)
                        {
                            foreach (var highlightWord in highlightWords)
                            {
                                product.Description = product.Description.Replace(highlightWord, $"<em>{highlightWord}</em>", StringComparison.OrdinalIgnoreCase);
                            }
                        }
                    }

                    finalResult = new Result()
                    {
                        FilteredProducts = filteredProducts,
                        Filter = filterObject
                    };
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    };

                    _cache.Set(cacheKey, finalResult, cacheOptions);


                    return Ok(finalResult);
                }
                else
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                return StatusCode(500, new { Message = "An error occurred while processing your request.", Error = ex.Message });
            }
        }

        /// <summary>
        /// Extracts the 10 most common words from product descriptions, skipping the top 5.
        /// </summary>
        /// <param name="descriptions">List of product descriptions.</param>
        /// <returns>Array of the 10 most common words.</returns>
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

    public class RootObject
    {
        [JsonPropertyName("products")]
        public List<Product> Products { get; set; }
        public ApiKeys ApiKeys { get; set; }
    }

    public class ApiKeys
    {
        public string Primary { get; set; }
        public string Secondary { get; set; }
    }

    public class Result
    {
        public List<Product> FilteredProducts { get; set; }
        public Filter Filter { get; set; }

    }

    public class Filter
    {
        public Decimal? MinPrice { get; set; }
        public Decimal? MaxPrice { get; set; }
        public String[] Sizes { get; set; }
        public String[] CommonWords { get; set; }
    }
}
