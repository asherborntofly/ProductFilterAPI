using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using ProductFilterAPI.Controllers;
using ProductFilterAPI.Models;
using System.Net;
using System.Text;
using System.Text.Json;


public class ProductsControllerTests
{
    private Mock<HttpMessageHandler> _handlerMock;
    private Mock<IHttpClientFactory> _httpClientFactoryMock;
    private Mock<ILogger<ProductsController>> _loggerMock;
    private Mock<IMemoryCache> _cacheMock;
    private ProductsController _controller;

    public ProductsControllerTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<ProductsController>>();
        _cacheMock = new Mock<IMemoryCache>();

        var httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://localhost/")
        };

        _httpClientFactoryMock
            .Setup(_ => _.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        _controller = new ProductsController(_httpClientFactoryMock.Object, _loggerMock.Object, _cacheMock.Object);
    }

    private void SetupHttpClientMock(string jsonProducts)
    {
        // Mock HttpClient
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonProducts, Encoding.UTF8, "application/json"),
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object);

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        // Mock ILogger
        var loggerMock = new Mock<ILogger<ProductsController>>();

        // Use real MemoryCache
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        // Pass dependencies to the controller
        _controller = new ProductsController(httpClientFactoryMock.Object, loggerMock.Object, memoryCache);
    }


    [Fact]
    public async Task GetFilteredProducts_NoParameters_ReturnsAllProducts()
    {
        var products = Enumerable.Range(1, 48).Select(i => new Product
        {
            Title = $"Product{i}",
            Price = i * 10,
            Sizes = new List<string> { "medium" },
            Description = $"Description for Product{i}."
        }).ToList();

        SetupHttpClientMock(JsonSerializer.Serialize(new RootObject { Products = products }));
        var result = await _controller.GetFilteredProducts(null, null, null, null) as OkObjectResult;

        Assert.NotNull(result);
        var response = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsAssignableFrom<Result>(response.Value);
        var returnedProducts = Assert.IsAssignableFrom<List<Product>>(returnedResult.FilteredProducts);
        Assert.Equal(48, returnedProducts.Count);
    }

    [Fact]
    public async Task GetFilteredProducts_WithMinPrice_ReturnsFilteredProducts()
    {
        var products = new List<Product>
        {
            new Product { Title = "Product1", Price = 10, Sizes = new List<string> { "medium" }, Description = "A great product." },
            new Product { Title = "Product2", Price = 20, Sizes = new List<string> { "large" }, Description = "Another great product." }
        };

        SetupHttpClientMock(JsonSerializer.Serialize(new RootObject { Products = products }));

        var result = await _controller.GetFilteredProducts(15, null, null, null) as OkObjectResult;

        Assert.NotNull(result);
        var response = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsAssignableFrom<Result>(response.Value);
        var returnedProducts = Assert.IsAssignableFrom<List<Product>>(returnedResult.FilteredProducts);
        Assert.Single(returnedProducts);
        Assert.Equal("Product2", returnedProducts[0].Title);
    }

    [Fact]
    public async Task GetFilteredProducts_WithFilters_ReturnsFilteredProducts()
    {
        var products = new List<Product>
        {
            new Product { Title = "Product1", Price = 10, Sizes = new List<string> { "medium" }, Description = "A great product." },
            new Product { Title = "Product2", Price = 20, Sizes = new List<string> { "large" }, Description = "Another great product." },
            new Product { Title = "Product3", Price = 30, Sizes = new List<string> { "medium" }, Description = "Yet another great product." }
        };

        SetupHttpClientMock(JsonSerializer.Serialize(new RootObject { Products = products }));

        var result = await _controller.GetFilteredProducts(15, null, "medium", null) as OkObjectResult;

        Assert.NotNull(result);
        var response = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsAssignableFrom<Result>(response.Value);
        var returnedProducts = Assert.IsAssignableFrom<List<Product>>(returnedResult.FilteredProducts);
        Assert.Single(returnedProducts);
        Assert.Equal("Product3", returnedProducts[0].Title);
    }

    [Fact]
    public async Task GetFilteredProducts_NoMatchingFilters_ReturnsEmptyResult()
    {
        var products = new List<Product>
        {
            new Product { Title = "Product1", Price = 10, Sizes = new List<string> { "medium" }, Description = "A great product." }
        };

        SetupHttpClientMock(JsonSerializer.Serialize(new RootObject { Products = products }));

        var result = await _controller.GetFilteredProducts(50, null, null, null) as OkObjectResult;

        Assert.NotNull(result);
        var response = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsAssignableFrom<Result>(response.Value);
        var returnedProducts = Assert.IsAssignableFrom<List<Product>>(returnedResult.FilteredProducts);
        Assert.Empty(returnedProducts);
    }

}
