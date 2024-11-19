using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductFilterAPI.Controllers;
using ProductFilterAPI.Models;


public class ProductsControllerTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<ILogger<ProductsController>> _loggerMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _productServiceMock = new Mock<IProductService>();
        _loggerMock = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_productServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetFilteredProducts_NoParameters_ReturnsAllProducts()
    {
        // Arrange
        var expectedProducts = Enumerable.Range(1, 48).Select(i => new Product
        {
            Title = $"Product{i}",
            Price = i * 10,
            Sizes = new List<string> { "medium" },
            Description = $"Description for Product{i}."
        }).ToList();

        var expectedResponse = new ResultResponse
        {
            FilteredProducts = expectedProducts,
            Filter = new Filter
            {
                MinPrice = 10,
                MaxPrice = 480,
                Sizes = new[] { "medium" }
            }
        };

        _productServiceMock
            .Setup(x => x.GetFilteredProductsAsync(null, null, null, null))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetFilteredProducts(null, null, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedResponse = Assert.IsType<ResultResponse>(okResult.Value);
        Assert.Equal(48, returnedResponse.FilteredProducts.Count);
        _productServiceMock.Verify(x => x.GetFilteredProductsAsync(null, null, null, null), Times.Once);
    }

    [Fact]
    public async Task GetFilteredProducts_WithMinPrice_ReturnsFilteredProducts()
    {
        // Arrange
        var expectedProducts = new List<Product>
        {
            new Product { Title = "Product2", Price = 20, Sizes = new List<string> { "large" }, Description = "Another great product." }
        };

        var expectedResponse = new ResultResponse
        {
            FilteredProducts = expectedProducts,
            Filter = new Filter
            {
                MinPrice = 20,
                MaxPrice = 20,
                Sizes = new[] { "large" }
            }
        };

        _productServiceMock
            .Setup(x => x.GetFilteredProductsAsync(15, null, null, null))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetFilteredProducts(15, null, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedResponse = Assert.IsType<ResultResponse>(okResult.Value);
        Assert.Single(returnedResponse.FilteredProducts);
        Assert.Equal("Product2", returnedResponse.FilteredProducts[0].Title);
        _productServiceMock.Verify(x => x.GetFilteredProductsAsync(15, null, null, null), Times.Once);
    }

    [Fact]
    public async Task GetFilteredProducts_WithFilters_ReturnsFilteredProducts()
    {
        // Arrange
        var expectedProducts = new List<Product>
        {
            new Product { Title = "Product3", Price = 30, Sizes = new List<string> { "medium" }, Description = "Yet another great product." }
        };

        var expectedResponse = new ResultResponse
        {
            FilteredProducts = expectedProducts,
            Filter = new Filter
            {
                MinPrice = 30,
                MaxPrice = 30,
                Sizes = new[] { "medium" }
            }
        };

        _productServiceMock
            .Setup(x => x.GetFilteredProductsAsync(15, null, "medium", null))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetFilteredProducts(15, null, "medium", null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedResponse = Assert.IsType<ResultResponse>(okResult.Value);
        Assert.Single(returnedResponse.FilteredProducts);
        Assert.Equal("Product3", returnedResponse.FilteredProducts[0].Title);
    }

    [Fact]
    public async Task GetFilteredProducts_WhenExceptionOccurs_ReturnsInternalServerError()
    {
        // Arrange
        _productServiceMock
            .Setup(x => x.GetFilteredProductsAsync(It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.GetFilteredProducts(null, null, null, null);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(statusCodeResult.Value);
        Assert.Equal("An error occurred while processing your request", problemDetails.Title);
    }
}
