// C#
using AlzaShopApi.Controllers.v2;
using AlzaShopApi.Models;
using AlzaShopApi.Models.Database;
using AlzaShopApi.Services.Interfaces;
using AlzaShopApi.Toolkit;
using AlzaShopApi.Toolkit.Brokers.Interfaces;
using AlzaShopApi.Views;
using Czlovek.RabbitMq.Base.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;
using AlzaShopApi;
using Asp.Versioning;
using System.Reflection;

namespace Tests;

/// <summary>
/// Test class for the <c>ProductController</c> handling API Version 2.
/// </summary>
/// <remarks>
/// Contains unit tests validating the functionality of the ProductController in version 2.
/// Primarily ensures the correctness of CRUD operations, validation logic, error handling,
/// API versioning, and integration with dependent services and messaging brokers.
/// </remarks>
/// <seealso cref="ILogger{ProductController}"/>
/// <seealso cref="IProductService"/>
/// <seealso cref="IMessageBroker"/>
/// <seealso cref="IRabbitMqDefaultClient"/>
public class ProductControllerV2Tests
{
    /// Mock of the IProductService interface used for unit testing.
    /// This mock is utilized to simulate the behavior of IProductService
    /// without requiring actual service implementations or dependencies.
    /// It is primarily used in the ProductControllerV2Tests to test
    /// controller behavior under various scenarios.
    private readonly Mock<IProductService> _productServiceMock;

    /// <summary>
    /// Mock of the IMessageBroker interface used for unit testing.
    /// </summary>
    /// <remarks>
    /// This mock object is utilized to simulate the behavior of a message broker
    /// in test scenarios, enabling the verification of message-sending operations
    /// without relying on an actual implementation.
    /// </remarks>
    private readonly Mock<IMessageBroker> _messageBrokerMock;

    /// <summary>
    /// Instance of the ProductController, used for unit testing of ProductControllerV2 functionalities.
    /// </summary>
    /// <remarks>
    /// This private readonly field is initialized with a new instance of ProductController using mocked dependencies,
    /// including ILogger, IProductService, IMessageBroker, and IRabbitMqDefaultClient.
    /// </remarks>
    private readonly ProductController _controller;

    /// Unit tests for the ProductController class in version 2 of the API.
    /// Tests cover various functionalities including fetching, creating, updating,
    /// and error handling behavior of the ProductController.
    /// All dependencies are mocked to isolate the functionality of the controller under test.
    /// Includes tests for behaviors unique to v2 such as asynchronous operations
    /// and specific paging behavior.
    /// Constructor initializes mock dependencies for ILogger,
    /// IProductService, IMessageBroker, and IRabbitMqDefaultClient.
    /// These mocked dependencies are used to verify the interaction logic
    /// between the controller and external services.
    /// Test coverage:
    /// - Tests for different scenarios in the Get endpoints, including handling
    /// pagination, empty responses, errors, and general product retrieval.
    /// - Tests for the Post endpoint, ensuring valid products are added correctly
    /// and invalid data is handled appropriately.
    /// - Tests for the Put endpoint, focusing on valid and invalid update scenarios,
    /// including cases when products do not exist or broker operations fail.
    /// - Validation of API versioning and parameter passing for pagination.
    /// - Special case tests that validate asynchronous behavior,
    /// error propagation, and messaging integration in v2.
    public ProductControllerV2Tests()
    {
        Mock<ILogger<ProductController>> loggerMock = new Mock<ILogger<ProductController>>();
        _productServiceMock = new Mock<IProductService>();
        _messageBrokerMock = new Mock<IMessageBroker>();
        Mock<IRabbitMqDefaultClient> rabbitMqDefaultClientMock = new Mock<IRabbitMqDefaultClient>();
        _controller = new ProductController(
            loggerMock.Object,
            _productServiceMock.Object,
            _messageBrokerMock.Object,
            rabbitMqDefaultClientMock.Object);
    }

    /// Validates that the Get method of the ProductController returns a valid Ok response
    /// when provided with a valid product ID. Specifically, it verifies that the returned product data
    /// matches the expected property values from the mock product data.
    /// Returns an OkObjectResult containing the product details when a valid product ID is passed.
    /// The returned result is tested against expected product properties to ensure correctness.
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

    /// Verifies that the Get method of the ProductController returns a NoContent response
    /// when a product with a non-existent ID is requested.
    /// This method sets up the mocked IProductService to return null for a given ID
    /// that does not correspond to an existing product. It asserts that the result
    /// of the controller's Get method is of type NoContentResult.
    /// <returns>
    /// A task representing the asynchronous unit test operation.
    /// The test succeeds if the response returned by the Get method is NoContentResult.
    /// </returns>
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

    /// Tests that the `Get` method of the `ProductController` returns a `BadRequest` result
    /// when called with invalid input that triggers a validation error in the service layer.
    /// <returns>
    /// A task representing the asynchronous test operation. Ensures the method returns a
    /// `BadRequestObjectResult` with an appropriate validation error message.
    /// </returns>
    [Fact]
    public async Task Get_WithValidationError_ReturnsBadRequest()
    {
        // Arrange
        const int id = 0; // Invalid ID
        const bool availableOnly = true;

        _productServiceMock.Setup(s => s.GetProduct(id, availableOnly))
            .ThrowsAsync(new ValidationException("Id must be greater than 0."));

        // Act
        var result = await _controller.Get(id, availableOnly);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("greater than 0", badRequestResult.Value?.ToString() ?? string.Empty);
    }

    /// Tests the functionality of retrieving a paginated list of products using the associated ProductController.
    /// Ensures that the method returns an OkObjectResult with a list of products when called with valid pagination parameters.
    /// The method verifies the following:
    /// - The returned result is of type OkObjectResult.
    /// - The value of the result is of type List.
    /// - The list contains the expected number of products.
    /// - Each product in the result matches the expected data based on the provided mock setup.
    /// <return>Task representing the asynchronous operation, used to test functionality and assertions when the method is invoked.</return>
    [Fact]
    public async Task Get_ProductList_WithPagination_ReturnsOk()
    {
        // Arrange
        int? pageIndex = 0;
        int? pageLimit = 2;
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

        _productServiceMock.Setup(s => s.GetProductList(pageIndex, pageLimit, availableOnly))
            .Returns(products);

        // Act
        var result = await _controller.Get(pageIndex, pageLimit, availableOnly);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<ProductView>>(okResult.Value);

        Assert.Equal(2, returnValue.Count);
        Assert.Equal(1, returnValue[0].Id);
        Assert.Equal(2, returnValue[1].Id);
    }

    /// Validates that the Get method in ProductController uses the default page size
    /// defined in the Paging constants when no page limit is explicitly specified.
    /// This test ensures the following:
    /// 1. The default page size (Constants.Paging.PageSize) is used when pageLimit is null.
    /// 2. The GetProductList method of the IProductService is invoked with the correct arguments.
    /// 3. The response from the Get method is of type OkObjectResult.
    /// <return>
    /// A Task representing the asynchronous unit test execution.
    /// </return>
    [Fact]
    public async Task Get_ProductList_WithDefaultPageSize_UsesPagingConstant()
    {
        // Arrange
        int? pageIndex = 0;
        int? pageLimit = null;
        bool? availableOnly = true;

        var products = new List<Product>
        {
            new()
            {
                Id = 1,
                Name = "Product 1",
                Url = "http://product1.com",
                Price = 10.0m,
                StockQuantity = 5
            }
        }.ToAsyncEnumerable();

        _productServiceMock.Setup(s => s.GetProductList(pageIndex, Constants.Paging.PageSize, availableOnly))
            .Returns(products);

        // Act
        var result = await _controller.Get(pageIndex, pageLimit, availableOnly);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _productServiceMock.Verify(s => s.GetProductList(pageIndex, Constants.Paging.PageSize, availableOnly), Times.Once);
    }

    /// Verifies that when the product list is empty, the method returns a NoContentResult.
    /// This test sets up a scenario where the product service returns an empty asynchronous enumerable
    /// for the requested product list. It ensures that the controller handles this situation correctly
    /// by returning a 204 No Content HTTP response.
    /// <return>Void</return>
    [Fact]
    public async Task Get_EmptyProductList_ReturnsNoContent()
    {
        // Arrange
        int? pageIndex = 0;
        int? pageLimit = 10;
        bool? availableOnly = true;
        var emptyProducts = Array.Empty<Product>().ToAsyncEnumerable();

        _productServiceMock.Setup(s => s.GetProductList(pageIndex, pageLimit, availableOnly))
            .Returns(emptyProducts);

        // Act
        var result = await _controller.Get(pageIndex, pageLimit, availableOnly);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    /// Tests the POST endpoint of the ProductControllerV2 to ensure it returns an HTTP 200 OK status
    /// when provided with a valid product.
    /// This test ensures that the CreateProduct method of the IProductService is called with the
    /// correct parameters matching the input provided to the controller, and that the response
    /// is of the correct type.
    /// <return>Returns a completed task indicating the success or failure of the test.</return>
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

    /// Tests if posting a product with invalid data returns a BadRequest result.
    /// This method validates the response of the product creation endpoint
    /// when provided with an invalid product data, such as an incorrectly
    /// formatted URL.
    /// <returns>
    /// A Task representing the asynchronous operation. The result validates
    /// that a BadRequestObjectResult is returned and contains appropriate error details.
    /// </returns>
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

    /// Tests if the HTTP PUT operation returns an OK response when provided with a valid product update request.
    /// This method verifies that:
    /// - The product existence is ensured before attempting updates.
    /// - The stock quantity is updated using the product service.
    /// - The appropriate command is sent via the message broker.
    /// It asserts the following:
    /// - The result of the PUT operation is of type OkResult.
    /// - The `EnsureProductExists` method of the product service is invoked exactly once.
    /// - The `Send` method of the message broker is invoked with a command matching the expected ID and stock quantity.
    /// <returns>
    /// Asserts that the update operation completes successfully and returns an HTTP OK response.
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

        _productServiceMock.Setup(s => s.EnsureProductExists(updateProductView.Id))
            .Returns(Task.CompletedTask);

        _productServiceMock.Setup(s => s.LazyUpdateProductStockQuantity(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        _messageBrokerMock.Setup(m => m.Send(It.IsAny<ICommand>()));

        // Act
        var result = await _controller.Put(updateProductView);

        // Assert
        Assert.IsType<OkResult>(result);
        _productServiceMock.Verify(s => s.EnsureProductExists(updateProductView.Id), Times.Once);
        _messageBrokerMock.Verify(m => m.Send(
            It.Is<ICommand>(c => c is UpdateProductStockQuantityCommand &&
                                 ((UpdateProductStockQuantityCommand)c).Id == updateProductView.Id &&
                                 ((UpdateProductStockQuantityCommand)c).StockQuantity == updateProductView.StockQuantity)),
            Times.Once);
    }

    /// <summary>
    /// Validates updating a non-existent product in the product controller.
    /// Ensures a bad request response is returned when attempting to update a product
    /// that does not exist in the system.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, containing a bad request result if the product ID does not exist.</returns>
    [Fact]
    public async Task Put_WithNonExistentProduct_ReturnsBadRequest()
    {
        // Arrange
        var updateProductView = new UpdateProductView
        {
            Id = 999, // Non-existent ID
            StockQuantity = 20
        };

        _productServiceMock.Setup(s => s.EnsureProductExists(updateProductView.Id))
            .ThrowsAsync(new NotFoundException(updateProductView.Id));

        // Act
        var result = await _controller.Put(updateProductView);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("999", badRequestResult.Value?.ToString() ?? string.Empty);
    }

    /// Validates the update product request data and returns a BadRequest response if the data is invalid.
    /// This method ensures that the provided `UpdateProductView` model adheres to validation rules.
    /// If the `Id` is invalid (e.g., less than 1), it triggers a validation error resulting in a BadRequest.
    /// <returns>
    /// A `BadRequestObjectResult` response if the data is invalid, with the validation error details.
    /// </returns>
    [Fact]
    public async Task Put_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var updateProductView = new UpdateProductView
        {
            Id = 0, // Invalid ID (less than 1)
            StockQuantity = 20
        };

        _productServiceMock.Setup(s => s.EnsureProductExists(updateProductView.Id))
            .ThrowsAsync(new ValidationException("Id must be greater than 0."));

        // Act
        var result = await _controller.Put(updateProductView);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("greater than 0", badRequestResult.Value?.ToString() ?? string.Empty);
    }

    /// Verifies that the `Put` method in `ProductController` version 2.0 uses the `IMessageBroker` to enqueue
    /// commands instead of updating the product stock quantity directly via the `IProductService`.
    /// <return>
    /// A task that represents the asynchronous operation for the test. The task result confirms that:
    /// - The `IMessageBroker.Send` method is invoked exactly once.
    /// - The `IProductService.UpdateProductStockQuantity` method is not called.
    /// - The `Put` method returns an `OkResult`.
    /// </return>
    [Fact]
    public async Task Put_DifferentFromV1_UsesEnqueueInsteadOfDirect()
    {
        // Arrange
        var updateProductView = new UpdateProductView
        {
            Id = 1,
            StockQuantity = 20
        };

        _productServiceMock.Setup(s => s.EnsureProductExists(updateProductView.Id))
            .Returns(Task.CompletedTask);

        _messageBrokerMock.Setup(m => m.Send(It.IsAny<ICommand>()));

        // Act
        var result = await _controller.Put(updateProductView);

        // Assert
        Assert.IsType<OkResult>(result);

        // V2 should use message broker instead of direct update
        _messageBrokerMock.Verify(m => m.Send(It.IsAny<ICommand>()), Times.Once);

        // V2 should NOT call UpdateProductStockQuantity directly
        _productServiceMock.Verify(s => s.UpdateProductStockQuantity(It.IsAny<Product>()), Times.Never);
    }

    //[Fact]
    //public async Task Get_WithNegativePageIndex_ReturnsBadRequest()
    //{
    //    // Arrange
    //    int? pageIndex = -1;
    //    int? pageLimit = 10;
    //    bool? availableOnly = true;

    //    _productServiceMock.Setup(s => s.GetProductList(pageIndex, pageLimit, availableOnly))
    //        .ThrowsAsync(new ValidationException("Page index cannot be negative."));

    //    // Act
    //    var result = await _controller.Get(pageIndex, pageLimit, availableOnly);

    //    // Assert
    //    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    //    Assert.Contains("cannot be negative", badRequestResult.Value?.ToString() ?? string.Empty);
    //}

    //[Fact]
    //public async Task Get_WithZeroPageLimit_ReturnsBadRequest()
    //{
    //    // Arrange
    //    int? pageIndex = 0;
    //    int? pageLimit = 0;
    //    bool? availableOnly = true;

    //    _productServiceMock.Setup(s => s.GetProductList(pageIndex, pageLimit, availableOnly))
    //        .ThrowsAsync(new ValidationException("Page limit must be greater than 0."));

    //    // Act
    //    var result = await _controller.Get(pageIndex, pageLimit, availableOnly);

    //    // Assert
    //    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    //    Assert.Contains("greater than 0", badRequestResult.Value?.ToString() ?? string.Empty);
    //}

    //[Fact]
    //public async Task Get_WithServerError_InProductList_ReturnsProblemDetails()
    //{
    //    // Arrange
    //    int? pageIndex = 0;
    //    int? pageLimit = 10;
    //    bool? availableOnly = true;

    //    _productServiceMock.Setup(s => s.GetProductList(pageIndex, pageLimit, availableOnly))
    //        .ThrowsAsync(new Exception("Database connection error"));

    //    // Act
    //    var result = await _controller.Get(pageIndex, pageLimit, availableOnly);

    //    // Assert
    //    var objectResult = Assert.IsType<ObjectResult>(result);
    //    Assert.Equal(500, objectResult.StatusCode);
    //}

    /// Verifies that the Get method returns a ProblemDetails object with a 500 status code when a server error occurs during the GetProduct operation.
    /// This test simulates a server error in the GetProduct execution and ensures that the controller appropriately returns
    /// an ObjectResult containing a 500 status code.
    /// <return>
    /// A ProblemDetails structure with a 500 status code, indicating an internal server error has occurred.
    /// </return>
    [Fact]
    public async Task Get_WithServerError_InGetProduct_ReturnsProblemDetails()
    {
        // Arrange
        const int id = 1;
        const bool availableOnly = true;

        _productServiceMock.Setup(s => s.GetProduct(id, availableOnly))
            .ThrowsAsync(new Exception("Database connection error"));

        // Act
        var result = await _controller.Get(id, availableOnly);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    /// Verifies that when a very large page index is provided, the method returns an empty list and yields a NoContent result.
    /// <returns>Returns a task representing the asynchronous operation, with the result being of type NoContentResult when no products are found for the specified page.</returns>
    [Fact]
    public async Task Get_WithLargePageIndex_ReturnsEmptyList()
    {
        // Arrange
        int? pageIndex = 1000; // Very large page number that shouldn't have results
        int? pageLimit = 10;
        bool? availableOnly = true;

        var emptyProducts = Array.Empty<Product>().ToAsyncEnumerable();

        _productServiceMock.Setup(s => s.GetProductList(pageIndex, pageLimit, availableOnly))
            .Returns(emptyProducts);

        // Act
        var result = await _controller.Get(pageIndex, pageLimit, availableOnly);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    /// Verifies that when the message broker fails while attempting to send a command
    /// during a product update, the controller returns a ProblemDetails response with
    /// a status code of 500 (Internal Server Error).
    /// <return>
    /// A task representing the asynchronous operation. The result contains the
    /// assertion validating that the controller returns an ObjectResult with a status
    /// code of 500 when the message broker fails.
    /// </return>
    [Fact]
    public async Task Put_WhenMessageBrokerFails_ReturnsProblemDetails()
    {
        // Arrange
        var updateProductView = new UpdateProductView
        {
            Id = 1,
            StockQuantity = 20
        };

        _productServiceMock.Setup(s => s.EnsureProductExists(updateProductView.Id))
            .Returns(Task.CompletedTask);

        _messageBrokerMock.Setup(m => m.Send(It.IsAny<ICommand>()))
            .Throws(new Exception("Message broker connection failed"));

        // Act
        var result = await _controller.Put(updateProductView);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    /// <summary>
    /// Verifies that the Post method of the ProductController returns appropriate ProblemDetails when the service throws an unexpected error.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an ObjectResult with HTTP status code 500 when an unexpected error occurs in the service.
    /// </returns>
    [Fact]
    public async Task Post_WhenServiceThrowsUnexpectedError_ReturnsProblemDetails()
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
            .ThrowsAsync(new Exception("Unexpected database error"));

        // Act
        var result = await _controller.Post(createProductView);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    /// Validates that the ProductController class is decorated with the appropriate
    /// API version attribute and that the specified API version matches the expected version.
    /// Ensures the controller adheres to the API versioning standards.
    /// Tests:
    /// - Checks if the `ApiVersionAttribute` is present on the `ProductController`.
    /// - Verifies that the version "2.0" is included in the defined API versions.
    /// Assertions:
    /// - Confirms the attribute is not null.
    /// - Confirms the expected version is part of the defined versions.
    [Fact]
    public void Controller_HasCorrectApiVersion()
    {
        // Arrange & Act
        var apiVersionAttribute = typeof(ProductController).GetCustomAttribute(
            typeof(ApiVersionAttribute), false) as ApiVersionAttribute;

        // Assert
        Assert.NotNull(apiVersionAttribute);
        Assert.Contains("2.0", apiVersionAttribute.Versions.Select(v => v.ToString()));
    }

    /// Verifies that pagination parameters are passed correctly
    /// to the ProductService's GetProductList method during a GET request
    /// to the ProductController.
    /// This test ensures that the exact values for pageIndex,
    /// pageLimit, and availableOnly are passed to the mocked
    /// IProductService.
    /// <returns>Task representing the asynchronous operation of the test.</returns>
    [Fact]
    public async Task Get_PaginationParameters_ArePassedCorrectly()
    {
        // Arrange
        int? pageIndex = 2;
        int? pageLimit = 15;
        bool? availableOnly = true;

        var emptyProducts = Array.Empty<Product>().ToAsyncEnumerable();

        _productServiceMock.Setup(s => s.GetProductList(pageIndex, pageLimit, availableOnly))
            .Returns(emptyProducts);

        // Act
        await _controller.Get(pageIndex, pageLimit, availableOnly);

        // Assert - verify exact parameters are passed to service
        _productServiceMock.Verify(s => s.GetProductList(2, 15, true), Times.Once);
    }

    /// Tests if the `Get` method in `ProductController` correctly processes an asynchronous enumerable of products returned by the product service.
    /// This test verifies the following:
    /// - The service `GetProductList` method is called with the proper parameters.
    /// - The `Get` method correctly handles an asynchronous enumerable and transforms it into a list of `ProductView` objects.
    /// - The final result is returned as an `OkObjectResult` with the expected number of items in the response.
    /// Ensures the controller can handle async streams efficiently and produce the expected output.
    /// <return>Returns a Task that represents the asynchronous test operation.</return>
    [Fact]
    public async Task Get_ProductList_ProcessesAsyncEnumerableCorrectly()
    {
        // Arrange
        int? pageIndex = 0;
        int? pageLimit = 3;
        bool? availableOnly = true;

        // Create an async enumerable that yields elements one by one
        async IAsyncEnumerable<Product> GetProductsAsync()
        {
            await Task.Delay(1); // Simulate async behavior
            yield return new Product { Id = 1, Name = "Product 1", Url = "http://p1.com", StockQuantity = 5 };

            await Task.Delay(1); // Simulate async behavior
            yield return new Product { Id = 2, Name = "Product 2", Url = "http://p2.com", StockQuantity = 10 };

            await Task.Delay(1); // Simulate async behavior
            yield return new Product { Id = 3, Name = "Product 3", Url = "http://p3.com", StockQuantity = 15 };
        }

        _productServiceMock.Setup(s => s.GetProductList(pageIndex, pageLimit, availableOnly))
            .Returns(GetProductsAsync());

        // Act
        var result = await _controller.Get(pageIndex, pageLimit, availableOnly);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<ProductView>>(okResult.Value);

        Assert.Equal(3, returnValue.Count);
        Assert.Equal(1, returnValue[0].Id);
        Assert.Equal(2, returnValue[1].Id);
        Assert.Equal(3, returnValue[2].Id);
    }
}

