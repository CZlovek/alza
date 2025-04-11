// C#
using AlzaShopApi.Controllers.v1;
using AlzaShopApi.Models.Database;
using AlzaShopApi.Services.Interfaces;
using AlzaShopApi.Toolkit;
using AlzaShopApi.Toolkit.Brokers.Interfaces;
using AlzaShopApi.Views;
using Czlovek.RabbitMq.Base.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Tests;

/// <summary>
/// Contains test cases for the ProductController in version 1 of the API.
/// </summary>
/// <remarks>
/// This class is designed to test various behaviors and functionality of the
/// ProductController, including GET, POST, PUT methods, error handling, and
/// endpoint attribute configurations. Mock dependencies are used for isolation.
/// </remarks>
/// <example>
/// Includes test methods that verify responses for valid and invalid input,
/// correct service usage, and expected behavior in error scenarios.
/// </example>
public class ProductControllerV1Tests
{
    /// <summary>
    /// Mock instance of the IProductService interface used for simulating behavior of the product service
    /// during unit testing of the ProductController.
    /// </summary>
    private readonly Mock<IProductService> _productServiceMock;

    /// <summary>
    /// An instance of the <see cref="ProductController"/> class used for handling
    /// product-related API operations during unit tests.
    /// </summary>
    private readonly ProductController _controller;

    /// Provides unit tests for the ProductController API of version 1.0.
    /// These tests verify the behavior of the ProductController, covering methods such as Get, Post, and Put.
    /// They check for proper response codes and error handling mechanisms for different scenarios.
    public ProductControllerV1Tests()
    {
        Mock<ILogger<ProductController>> loggerMock = new Mock<ILogger<ProductController>>();
        _productServiceMock = new Mock<IProductService>();
        Mock<IMessageBroker> messageBrokerMock = new Mock<IMessageBroker>();
        Mock<IRabbitMqDefaultClient> rabbitMqDefaultClientMock = new Mock<IRabbitMqDefaultClient>();
        _controller = new ProductController(
            loggerMock.Object,
            _productServiceMock.Object,
            messageBrokerMock.Object,
            rabbitMqDefaultClientMock.Object);
    }

    /// Verifies that the `Get` method of the `ProductController` successfully returns an HTTP 200 (OK)
    /// response when provided with a valid product ID. Ensures that the returned product data matches
    /// the expected values.
    /// Test Case Description:
    /// - Arrange: A valid product ID and a mock product instance are set up. The mock `IProductService`
    /// is configured to return the expected product.
    /// - Act: The `Get` method of the `ProductController` is invoked with the valid product ID and query parameter.
    /// - Assert: Validates that the response is of type `OkObjectResult` and contains a `ProductView` object with the
    /// expected property values.
    /// Parameters:
    /// - None
    /// Throws:
    /// - Does not throw. Verifies behavior using assertion methods within the unit test framework.
    [Fact]
    public async Task Get_WithValidId_ReturnsOk()
    {
        // Arrange
        const int id = 1;
        const bool availableOnly = true;

        var product = new Product
        {
            Id = id,
            Name = "Test Product",
            Url = "http://test.com",
            Price = 10.0m,
            StockQuantity = 5,
            Description = "Test Description"
        };

        _productServiceMock.Setup(s => s.GetProduct(id, availableOnly))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.Get(id, availableOnly);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ProductView>(okResult.Value);

        Assert.Equal(product.Id, returnValue.Id);
        Assert.Equal(product.Name, returnValue.Name);
        Assert.Equal(product.Url, returnValue.Url);
        Assert.Equal(product.Price, returnValue.Price);
        Assert.Equal(product.StockQuantity, returnValue.StockQuantity);
        Assert.Equal(product.Description, returnValue.Description);
    }

    /// Verifies that the `Get` method of `ProductController` returns a `NoContentResult`
    /// when a product with a non-existent ID is requested.
    /// The test simulates a scenario where the product does not exist in the data store.
    /// It mocks the `IProductService.GetProduct` method to return `null` for the given ID.
    /// Test Steps:
    /// 1. Arrange: Sets up the mock for `IProductService` to return `null` for the specified product ID.
    /// 2. Act: Calls the `Get` method of `ProductController` with the specified ID.
    /// 3. Assert: Verifies that the returned result is of type `NoContentResult`.
    /// Use case: This test ensures the API correctly returns a 204 No Content status when
    /// querying a product that does not exist.
    [Fact]
    public async Task Get_WithNonExistentId_ReturnsNoContent()
    {
        // Arrange
        const int id = 999;
        const bool availableOnly = true;

        _productServiceMock.Setup(s => s.GetProduct(id, availableOnly))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.Get(id, availableOnly);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    /// Tests that the Get method of the ProductController successfully returns
    /// an OK status result along with a list of products when requested with
    /// valid parameters.
    /// The test ensures:
    /// - The controller calls the product service to retrieve a list of products.
    /// - An OK result is returned upon successful retrieval.
    /// - The returned value contains the correct number of products with proper details.
    /// <return>
    /// A task representing the asynchronous operation indicating the outcome of the test.
    /// </return>
    [Fact]
    public async Task Get_ProductList_ReturnsOk()
    {
        // Arrange
        bool? availableOnly = true;

        var products = new List<Product>
        {
            new()
            {
                Id = 1,
                Name = "Product 1",
                Url = "http://product1.com",
                Price = 10.0m,
                StockQuantity = 5,
                Description = "Description 1"
            },
            new()
            {
                Id = 2,
                Name = "Product 2",
                Url = "http://product2.com",
                Price = 20.0m,
                StockQuantity = 10,
                Description = "Description 2"
            }
        }.ToAsyncEnumerable();

        _productServiceMock.Setup(s => s.GetProductList(null, null, availableOnly))
            .Returns(products);

        // Act
        var result = await _controller.Get(availableOnly);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<ProductView>>(okResult.Value);

        Assert.Equal(2, returnValue.Count);
        Assert.Equal(1, returnValue[0].Id);
        Assert.Equal(2, returnValue[1].Id);
    }

    /// Verifies that when the product list is empty, the API returns a NoContent (204) response.
    /// <returns>
    /// A task representing the asynchronous operation that asserts the response is of type NoContentResult.
    /// </returns>
    [Fact]
    public async Task Get_EmptyProductList_ReturnsNoContent()
    {
        // Arrange
        bool? availableOnly = true;
        var emptyProducts = Array.Empty<Product>().ToAsyncEnumerable();

        _productServiceMock.Setup(s => s.GetProductList(null, null, availableOnly))
            .Returns(emptyProducts);

        // Act
        var result = await _controller.Get(availableOnly);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    /// Verifies that posting a valid product returns an OkResult response and ensures that the CreateProduct method
    /// of the product service is called with the expected parameters.
    /// <return>
    /// A Task representing the asynchronous unit test operation.
    /// </return>
    [Fact]
    public async Task Post_WithValidProduct_ReturnsOk()
    {
        // Arrange
        var createProductView = new CreateProductView
        {
            Name = "New Product",
            Url = "http://newproduct.com",
            Price = 15.0m,
            StockQuantity = 8,
            Description = "A new test product"
        };

        _productServiceMock.Setup(s => s.CreateProduct(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Post(createProductView);

        // Assert
        Assert.IsType<OkResult>(result);
        _productServiceMock.Verify(s => s.CreateProduct(
            It.Is<Product>(p =>
                p.Name == createProductView.Name &&
                p.Url == createProductView.Url &&
                p.Price == createProductView.Price &&
                p.StockQuantity == createProductView.StockQuantity &&
                p.Description == createProductView.Description
            )), Times.Once);
    }

    /// Verifies that when an invalid product is provided to the `Post` method of the controller,
    /// a `BadRequest` response is returned. This includes validating if the product properties do not meet
    /// the required constraints, such as an invalid URL format or missing required fields.
    /// <return>Task representing the asynchronous operation.</return>
    [Fact]
    public async Task Post_WithInvalidProduct_ReturnsBadRequest()
    {
        // Arrange
        var createProductView = new CreateProductView
        {
            Name = "New Product",
            Url = "invalid-url", // Invalid URL format
            Price = 15.0m,
            StockQuantity = 8,
            Description = "A new test product"
        };

        _productServiceMock.Setup(s => s.CreateProduct(It.IsAny<Product>()))
            .ThrowsAsync(new ValidationException("Url is required and must be a valid URL."));

        // Act
        var result = await _controller.Post(createProductView);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("valid URL", badRequestResult.Value?.ToString() ?? string.Empty);
    }

    /// Tests the PUT endpoint of ProductController when provided with a valid product.
    /// Ensures that the response is of type OkResult and that the product's stock quantity is updated properly.
    /// <returns>
    /// A Task representing the asynchronous test operation. Confirms that the endpoint returns OkResult for valid input.
    /// </returns>
    [Fact]
    public async Task Put_WithValidProduct_ReturnsOk()
    {
        // Arrange
        var updateProductView = new UpdateProductView
        {
            Id = 1,
            StockQuantity = 20
        };

        _productServiceMock.Setup(s => s.UpdateProductStockQuantity(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Put(updateProductView);

        // Assert
        Assert.IsType<OkResult>(result);
        _productServiceMock.Verify(s => s.UpdateProductStockQuantity(
            It.Is<Product>(p =>
                p.Id == updateProductView.Id &&
                p.StockQuantity == updateProductView.StockQuantity
            )), Times.Once);
    }

    /// Tests the behavior of the PUT endpoint when attempting to update a non-existent product.
    /// Verifies that a BadRequest result is returned with a corresponding error message.
    /// Ensures proper handling of cases where the product ID provided does not exist.
    /// <returns>
    /// A task that represents the asynchronous operation of the test. The task result contains
    /// a Boolean indicating that the PUT method returns a BadRequest result with the expected error message.
    /// </returns>
    [Fact]
    public async Task Put_WithNonExistentProduct_ReturnsBadRequest()
    {
        // Arrange
        var updateProductView = new UpdateProductView
        {
            Id = 999, // Non-existent ID
            StockQuantity = 20
        };

        _productServiceMock.Setup(s => s.UpdateProductStockQuantity(It.IsAny<Product>()))
            .ThrowsAsync(new NotFoundException(updateProductView.Id));

        // Act
        var result = await _controller.Put(updateProductView);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("999", badRequestResult.Value?.ToString() ?? string.Empty);
    }

    /// Tests if the `Put` method of the `ProductController` returns a `BadRequest` response
    /// when provided with invalid data.
    /// This test ensures that the controller validates the input and responds
    /// appropriately if the data does not meet the required constraints.
    /// <return>
    /// Returns a Task. Asserts that the result is a `BadRequestObjectResult`
    /// and contains the expected validation error message.
    /// </return>
    [Fact]
    public async Task Put_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var updateProductView = new UpdateProductView
        {
            Id = 0, // Invalid ID (less than 1)
            StockQuantity = 20
        };

        _productServiceMock.Setup(s => s.UpdateProductStockQuantity(It.IsAny<Product>()))
            .ThrowsAsync(new ValidationException("Id must be greater than 0."));

        // Act
        var result = await _controller.Put(updateProductView);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("greater than 0", badRequestResult.Value?.ToString() ?? string.Empty);
    }

    /// Tests that the Get method of ProductController returns a BadRequest response
    /// when a validation error occurs during the product retrieval process.
    /// This test simulates a scenario where the IProductService.GetProduct method
    /// throws a ValidationException. It verifies that the controller properly
    /// handles the exception by returning a response with status code 400 (Bad Request)
    /// and ensures the error message contains information about the validation issue.
    /// <return>
    /// A response with status code 400 (Bad Request) containing details about the validation error.
    /// </return>
    [Fact]
    public async Task Get_WithValidationError_ReturnsBadRequest()
    {
        // Arrange
        const int id = 1;
        const bool availableOnly = true;

        _productServiceMock.Setup(s => s.GetProduct(id, availableOnly))
            .ThrowsAsync(new ValidationException("Validation error occurred"));

        // Act
        var result = await _controller.Get(id, availableOnly);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Validation error", badRequestResult.Value?.ToString() ?? string.Empty);
    }

    /// Validates that the API returns a proper ProblemDetails response with a 500 status code
    /// when a server-side error occurs during the execution of the Get method.
    /// <returns>
    /// An ObjectResult with status code 500 containing ProblemDetails that describes the server error.
    /// </returns>
    [Fact]
    public async Task Get_WithServerError_ReturnsProblemDetails()
    {
        // Arrange
        const int id = 1;
        const bool availableOnly = true;

        _productServiceMock.Setup(s => s.GetProduct(id, availableOnly))
            .ThrowsAsync(new Exception("Server error occurred"));

        // Act
        var result = await _controller.Get(id, availableOnly);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    /// Validates that the Get endpoint for retrieving a product list handles server errors gracefully.
    /// This method simulates a server error during the retrieval of the product list and
    /// verifies that the response is returned as a ProblemDetails object with the correct status code.
    /// <return>
    /// Returns an ObjectResult containing a ProblemDetails object with a status code of 500 (Internal Server Error).
    /// </return>
    [Fact]
    public async Task Get_ProductList_WithServerError_ReturnsProblemDetails()
    {
        // Arrange
        bool? availableOnly = true;

        _productServiceMock.Setup(s => s.GetProductList(null, null, availableOnly))
            .Throws(new Exception("Server error occurred"));

        // Act
        var result = await _controller.Get(availableOnly);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    /// This test verifies that the `availableOnly` parameter is correctly passed as `false`
    /// when invoking the `GetProduct` method of the `IProductService` interface through the controller.
    /// <return>
    /// This test ensures that `IProductService.GetProduct` is called with the correct `availableOnly` value set to `false`.
    /// </return>
    [Fact]
    public async Task Get_WithFalseAvailableOnlyParameter_PassesCorrectValue()
    {
        // Arrange
        const int id = 1;
        const bool availableOnly = false; // Explicitly false to test this path

        var product = new Product
        {
            Id = id,
            Name = "Out of Stock Product",
            Url = "http://test.com",
            StockQuantity = 0 // A product that would only be returned with availableOnly=false
        };

        _productServiceMock.Setup(s => s.GetProduct(id, availableOnly))
            .ReturnsAsync(product);

        // Act
        await _controller.Get(id, availableOnly);

        // Assert
        _productServiceMock.Verify(s => s.GetProduct(id, false), Times.Once);
    }

    /// Tests that the POST action in the ProductController returns a ProblemDetails object with a 500 status code when a server error occurs.
    /// The purpose of this test is to ensure that the controller handles exceptions thrown by the IProductService properly, providing
    /// a standardized ProblemDetails response to the client indicating an internal server error.
    /// <returns>
    /// A Task representing the asynchronous unit test. The task will be completed when the test finishes,
    /// and it will verify that the returned IActionResult is of type ObjectResult with a status code of 500.
    /// </returns>
    [Fact]
    public async Task Post_WithServerError_ReturnsProblemDetails()
    {
        // Arrange
        var createProductView = new CreateProductView
        {
            Name = "New Product",
            Url = "http://newproduct.com",
            Price = 15.0m,
            StockQuantity = 8
        };

        _productServiceMock.Setup(s => s.CreateProduct(It.IsAny<Product>()))
            .ThrowsAsync(new Exception("Server error occurred"));

        // Act
        var result = await _controller.Post(createProductView);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    /// Tests whether the `Post` method of `ProductController` returns a BadRequest response
    /// when attempting to create a product with a negative stock quantity.
    /// This test verifies that the system properly validates the `StockQuantity` field
    /// to ensure it does not accept negative values and delivers a meaningful error response.
    /// <return>
    /// A task representing the asynchronous operation of the test. The result of the test is the validation
    /// that a `BadRequestObjectResult` is returned in response to a product creation request with invalid data.
    /// </return>
    [Fact]
    public async Task Post_WithNegativeStockQuantity_ReturnsBadRequest()
    {
        // Arrange
        var createProductView = new CreateProductView
        {
            Name = "New Product",
            Url = "http://newproduct.com",
            Price = 15.0m,
            StockQuantity = -5 // Negative stock quantity
        };

        _productServiceMock.Setup(s => s.CreateProduct(It.IsAny<Product>()))
            .ThrowsAsync(new ValidationException("Stock quantity cannot be negative."));

        // Act
        var result = await _controller.Post(createProductView);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("cannot be negative", badRequestResult.Value?.ToString() ?? string.Empty);
    }

    /// Tests the behavior of the Put endpoint when a null or invalid product is passed.
    /// This simulates a scenario where model binding results in an invalid product object.
    /// Validates that the endpoint returns a BadRequest response indicating the issue.
    /// <return>
    /// Asserts that the response is a BadRequestObjectResult with the expected error message.
    /// </return>
    [Fact]
    public async Task Put_WithNullProduct_ReturnsBadRequest()
    {
        // This would be caught by model binding, but we can simulate the controller behavior

        // Arrange
        var nullProduct = new UpdateProductView
        {
            Id = 0,
            StockQuantity = null
        };

        // Act & Assert
        var result = await _controller.Put(nullProduct);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Id must be greater than 0", badRequestResult.Value?.ToString() ?? string.Empty);
    }

    /// Tests whether the Put method of the ProductController correctly handles
    /// server errors by returning ProblemDetails with a 500 status code.
    /// This test validates that when an exception is thrown by the IProductService
    /// during the update of a product's stock quantity, the controller responds
    /// with an ObjectResult containing the appropriate status code.
    /// <return>
    /// A successful test ensures the Put method returns an ObjectResult
    /// with a status code of 500 to indicate a server error.
    /// </return>
    [Fact]
    public async Task Put_WithServerError_ReturnsProblemDetails()
    {
        // Arrange
        var updateProductView = new UpdateProductView
        {
            Id = 1,
            StockQuantity = 20
        };

        _productServiceMock.Setup(s => s.UpdateProductStockQuantity(It.IsAny<Product>()))
            .ThrowsAsync(new Exception("Server error occurred"));

        // Act
        var result = await _controller.Put(updateProductView);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    /// <summary>
    /// Verifies that the <see cref="ProductController"/> class is decorated with the <see cref="ApiControllerAttribute"/>.
    /// </summary>
    /// <remarks>
    /// This test ensures that the <see cref="ApiControllerAttribute"/> is present on the <see cref="ProductController"/>,
    /// which enforces specific API controller behaviors, such as model binding and validation features
    /// that are integral to API development in ASP.NET Core.
    /// </remarks>
    [Fact]
    public void Controller_HasApiControllerAttribute()
    {
        // Arrange & Act
        var hasAttribute = Attribute.IsDefined(typeof(ProductController), typeof(ApiControllerAttribute));

        // Assert
        Assert.True(hasAttribute);
    }

    /// Validates that the HTTP GET method in the ProductController has the correct attributes for retrieving a product by its ID.
    /// This includes verifying the route template, supported response types, and Swagger documentation attributes of the method.
    /// The test ensures the following:
    /// - The method has an [HttpGet] attribute with the correct route template "{id:int}".
    /// - The method includes a [Produces] attribute specifying "application/json" as the supported response content type.
    /// - The method has appropriate [SwaggerResponse] attributes that indicate:
    /// - 200 OK for a successful response.
    /// - 204 No Content when no product is found.
    /// - 400 Bad Request for validation errors.
    /// - 500 Internal Server Error for unexpected server errors.
    [Fact]
    public void HttpGet_Method_HasCorrectAttributesForProductById()
    {
        // Arrange
        var methodInfo = typeof(ProductController).GetMethod("Get", new[] { typeof(int), typeof(bool?) });

        // Act & Assert
        var httpGetAttribute = methodInfo?.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault() as HttpGetAttribute;
        Assert.NotNull(httpGetAttribute);
        Assert.Equal("{id:int}", httpGetAttribute.Template);

        var producesAttribute = methodInfo?.GetCustomAttributes(typeof(ProducesAttribute), false).FirstOrDefault() as ProducesAttribute;
        Assert.NotNull(producesAttribute);
        Assert.Contains("application/json", producesAttribute.ContentTypes);

        var swaggerResponses = methodInfo?.GetCustomAttributes(typeof(SwaggerResponseAttribute), false).Cast<SwaggerResponseAttribute>().ToList();
        Assert.NotNull(swaggerResponses);
        Assert.Equal(4, swaggerResponses.Count);
        Assert.Contains(swaggerResponses, r => r.StatusCode == StatusCodes.Status200OK);
        Assert.Contains(swaggerResponses, r => r.StatusCode == StatusCodes.Status204NoContent);
        Assert.Contains(swaggerResponses, r => r.StatusCode == StatusCodes.Status400BadRequest);
        Assert.Contains(swaggerResponses, r => r.StatusCode == StatusCodes.Status500InternalServerError);
    }
}

