using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductFilterAPI.Models;

namespace ProductFilterAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResultResponse>> GetFilteredProducts(
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? size,
            [FromQuery] string? highlight)
        {
            try
            {
                var result = await _productService.GetFilteredProductsAsync(minPrice, maxPrice, size, highlight);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request with parameters: " +
                               $"minPrice={minPrice}, maxPrice={maxPrice}, size={size}, highlight={highlight}");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "An error occurred while processing your request",
                        Detail = ex.Message
                    });
            }
        }
    }
}
