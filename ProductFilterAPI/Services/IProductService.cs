using ProductFilterAPI.Models;

/// <summary>
/// Service interface for handling product-related operations
/// </summary>
public interface IProductService
{
  /// <summary>
  /// Retrieves and filters products based on the provided criteria
  /// </summary>
  /// <param name="minPrice">Minimum price filter</param>
  /// <param name="maxPrice">Maximum price filter</param>
  /// <param name="size">Size filter</param>
  /// <param name="highlight">Words to highlight in product descriptions</param>
  /// <returns>A ResultResponse containing filtered products and filter metadata</returns>
  /// <exception cref="HttpRequestException">Thrown when the external API request fails</exception>
  /// <exception cref="JsonException">Thrown when the API response cannot be deserialized</exception>
  Task<ResultResponse> GetFilteredProductsAsync(
      decimal? minPrice = null,
      decimal? maxPrice = null,
      string? size = null,
      string? highlight = null);
}