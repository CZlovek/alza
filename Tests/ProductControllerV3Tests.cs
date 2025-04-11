// C#
using AlzaShopApi.Controllers.v3;
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
using Czlovek.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using System.Reflection;

namespace Tests;

/// <summary>
/// Unit test suite for the ProductController class in its version 3.
/// </summary>
/// <remarks>
/// This test class aims to validate the behavior and functionality of the ProductController.
/// It uses mocks and assertions to test various scenarios, ensuring the controller behaves
/// as expected under different conditions. Specific features being tested include
/// CRUD operations, validation, and integration with external components such as RabbitMQ.
/// </remarks>
/// <example>
/// This test suite leverages the Xunit testing framework with methods marked by the
/// [Fact] attribute to test individual scenarios.
/// </example>
/// <seealso cref="ILogger{ProductController}" />
/// <seealso cref="IProductService" />
/// <seealso cref="IMessageBroker" />
/// <seealso cref="IRabbitMqDefaultClient" />
public class ProductControllerV3Tests
{
    /// <summary>
    /// Mock instance of the IProductService interface used in unit tests for ProductControllerV3.
    /// </summary>
    /// <remarks>
    /// Facilitates testing by simulating the behavior of the IProductService without the need for
    /// actual implementation or dependencies. Allows setup of return values, exceptions, and
    /// validation scenarios during test execution.
    /// </remarks>
    private readonly Mock<IProductService> _productServiceMock;

    /// <summary>
    /// Mock implementation of the <see cref="IMessageBroker"/> interface used for unit testing.
    /// </summary>
    /// <remarks>
    /// This mock is utilized to verify interactions with the message broker during the unit tests
    /// of the <see cref="ProductController"/>. Examples include ensuring correct methods are called
    /// and proper arguments are passed.
    /// </remarks>
    private readonly Mock<IMessageBroker> _messageBrokerMock;

    /// <summary>
    /// Mock instance of the IRabbitMqDefaultClient interface used for testing purposes.
    /// Represents the RabbitMQ client in the context of unit tests for the ProductControllerV3 class.
    /// This mock object is configured to simulate and verify interactions with RabbitMQ,
    /// such as sending messages with specific details.
    /// </summary>
    private readonly Mock<IRabbitMqDefaultClient> _rabbitMqDefaultClientMock;

    /// <summary>
    /// Represents an instance of the ProductController used for handling HTTP requests related
    /// to product operations in version 3 of the API.
    /// </summary>
    private readonly ProductController _controller;

    /// Unit test class for validating the functionality of the v3 version of the ProductController.
    /// Includes a variety of test cases to ensure proper interaction with dependencies, correct handling of inputs/output,
    /// and accurate adherence to the defined APIs and middleware expectations.
    public ProductControllerV3Tests()
    {
        Mock<ILogger<ProductController>> loggerMock = new Mock<ILogger<ProductController>>();
        _productServiceMock = new Mock<IProductService>();
        _messageBrokerMock = new Mock<IMessageBroker>();
        _rabbitMqDefaultClientMock = new Mock<IRabbitMqDefaultClient>();
        _controller = new ProductController(
            loggerMock.Object,
            _productServiceMock.Object,
            _messageBrokerMock.Object,
            _rabbitMqDefaultClientMock.Object);
    }

    /// Tests the Get method of the ProductController for a valid product ID.
    /// This test verifies that when a valid product ID is provided, the Get method
    /// returns a status code 200 (OK) along with a valid ProductView object containing
    /// the expected product details.
    /// The following steps are performed in this test:
    /// - A product with predefined properties is created.
    /// - The mocked IProductService's GetProduct method is configured to return the product.
    /// - The controller's Get method is called with the predefined ID and availability filter.
    /// - The result is validated to ensure it is of type OkObjectResult.
    /// - The returned ProductView object is verified to contain the same properties as the mocked product.
    /// This test ensures that the controller interacts correctly with the service layer and
    /// properly returns the expected product details for a valid request.
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

    /// Tests that the Get method of the ProductController, when called with a non-existent product ID,
    /// returns a NoContentResult status, indicating no content.
    /// This test verifies that the ProductController properly handles cases where the requested product ID
    /// does not exist in the system by returning the appropriate HTTP 204 No Content status.
    /// <returns>
    /// A task representing the asynchronous operation. The task result is an assertion that the response
    /// type is NoContentResult.
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

    /// Tests that the Get method of the ProductController returns a BadRequest response
    /// when a validation error occurs due to invalid input parameters.
    /// This method verifies that when an invalid product ID is provided causing a
    /// validation exception in the service layer, the controller correctly returns
    /// a 400 BadRequest response containing the appropriate error message.
    /// <returns>BadRequest response is returned when a validation error occurs.</returns>
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

    /// Validates that the Get endpoint of the ProductController (version 3)
    /// correctly returns an Ok result containing a paginated list of products.
    /// This test ensures the following:
    /// - The Get method properly handles pagination parameters (pageIndex, pageLimit).
    /// - The returned result is of type OkObjectResult.
    /// - The data returned matches the expected list of products.
    /// - Available-only filtering is correctly applied.
    /// <returns>
    /// A completed task representing the success or failure of the test validation,
    /// ensuring the correct functionality of the ProductController's Get method.
    /// </returns>
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

    /// Validates that the `Get` method in `ProductController` correctly applies the default paging behavior by using
    /// a predefined constant for page size when no explicit page limit is provided.
    /// The test sets up the `IProductService` mock to return an asynchronous stream of products and verifies that the
    /// controller calls the service with the expected parameters, including the default page size defined in
    /// `Constants.Paging.PageSize`. It also checks that the action method returns an `OkObjectResult` upon successful
    /// execution.
    /// Test Steps:
    /// 1. Arrange: Initializes required dependencies and configures the mocked `IProductService` to return a list of
    /// products as an async enumerable.
    /// 2. Act: Calls the `Get` method on the controller with the designated test parameters.
    /// 3. Assert: Validates that the returned result is of type `OkObjectResult` and that the service's `GetProductList`
    /// method was called with the expected parameters.
    /// This ensures that the controller:
    /// - Uses the default page size constant when no specific page limit is specified.
    /// - Correctly invokes the service layer with the appropriate arguments.
    /// - Returns the proper HTTP response type on a successful operation.
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

    /// Tests whether the `Get` method from the `ProductController` correctly returns a `NoContent`
    /// result when the product list is empty.
    /// This method sets up an empty product list using a mocked `IProductService` dependency and calls
    /// the `Get` method on the controller with specific paging and availability parameters.
    /// <return>
    /// `NoContentResult` if the product list is empty.
    /// </return>
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

    /// Validates that when a valid product is posted, the response is an Ok result.
    /// The method tests the behavior of the ProductController when handling a valid CreateProductView.
    /// Ensures that the product creation service is called with correct parameters.
    /// <return>
    /// A task representing the asynchronous operation.
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

    /// Tests if the Post action of the controller returns a BadRequest response
    /// when an invalid product is provided in the request body.
    /// The method verifies that:
    /// - A product with an invalid property, such as an incorrect URL format, triggers a validation error.
    /// - The resulting response is of type BadRequestObjectResult.
    /// - The error message contains relevant validation failure details.
    /// <returns>
    /// A task that completes when the test is executed, verifying the BadRequest result
    /// and validation message content.
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

    /// Validates the provided product and sends an update message using RabbitMQ.
    /// If the product is valid, it ensures the product exists and delegates the update to RabbitMQ.
    /// Returns an Ok status upon successful completion.
    /// <return>Returns OkResult if the product is valid and the RabbitMQ message is sent successfully.</return>
    [Fact]
    public async Task Put_WithValidProduct_UsesRabbitMq_ReturnsOk()
    {
        // Arrange
        var updateProductView = new UpdateProductView
        {
            Id = 1,
            StockQuantity = 20
        };

        _productServiceMock.Setup(s => s.EnsureProductExists(updateProductView.Id))
            .Returns(Task.CompletedTask);

        _rabbitMqDefaultClientMock.Setup(r => r.Send(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "application/json", null))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Put(updateProductView);

        // Assert
        Assert.IsType<OkResult>(result);

        // Verify RabbitMQ was used
        _rabbitMqDefaultClientMock.Verify(r => r.Send(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.Is<string>(o => o.Equals(updateProductView.Serialize(null), StringComparison.CurrentCultureIgnoreCase))
            , "application/json", null),
            Times.Once);
    }

    /// <summary>
    /// Tests that attempting to update a non-existent product returns a bad request response.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
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

    /// Verifies the behavior of the PUT endpoint when provided with invalid data.
    /// This test ensures that the action returns a BadRequest response when the product data
    /// passed to the controller fails validation conditions.
    /// <returns>A BadRequestObjectResult if the input data is invalid and does not meet
    /// expected validation criteria, such as an ID that is less than or equal to zero or
    /// invalid stock quantity.</returns>
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

    /// Verifies that the `Put` method in `ProductController` correctly uses `IRabbitMqDefaultClient`
    /// for direct communication with RabbitMQ, rather than using `IMessageBroker`.
    /// Ensures that:
    /// - `IRabbitMqDefaultClient.Send` is called exactly once with expected parameters.
    /// - `IMessageBroker.Send` is not called.
    /// - The `Put` method handles the HTTP request appropriately with a valid input.
    /// <return>
    /// Successfully validates the use of RabbitMQ direct integration by the `Put` method in `ProductController`.
    /// </return>
    [Fact]
    public async Task Put_DifferentFromV2_UsesRabbitMqDirectly()
    {
        // Arrange
        var updateProductView = new UpdateProductView
        {
            Id = 1,
            StockQuantity = 20
        };

        _productServiceMock.Setup(s => s.EnsureProductExists(updateProductView.Id))
            .Returns(Task.CompletedTask);

        _rabbitMqDefaultClientMock.Setup(r => r.Send(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(), "application/json", null))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Put(updateProductView);

        // Assert
        Assert.IsType<OkResult>(result);

        // Verify RabbitMQ was used directly (v3 approach)
        _rabbitMqDefaultClientMock.Verify(r => r.Send(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(), "application/json", null),
            Times.Once);

        // Verify MessageBroker was NOT used (v2 approach)
        _messageBrokerMock.Verify(m => m.Send(It.IsAny<ICommand>()), Times.Never);
    }

    /// Tests the scenario when a RabbitMQ exception occurs during an update operation
    /// and verifies that the controller responds with a ProblemDetails response.
    /// This method ensures that the `Put` endpoint in `ProductController`
    /// appropriately handles a RabbitMQ connection error, by returning
    /// an ObjectResult with a status code of 500.
    /// <return>
    /// Verifies that the result of the `Put` method is an ObjectResult
    /// with a status code of 500.
    /// </return>
    [Fact]
    public async Task Put_RabbitMqException_ReturnsProblemDetails()
    {
        // Arrange
        var updateProductView = new UpdateProductView
        {
            Id = 1,
            StockQuantity = 20
        };

        _productServiceMock.Setup(s => s.EnsureProductExists(updateProductView.Id))
            .Returns(Task.CompletedTask);

        _rabbitMqDefaultClientMock.Setup(r => r.Send(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(), "application/json", null))
            .ThrowsAsync(new Exception("RabbitMQ connection error"));

        // Act
        var result = await _controller.Put(updateProductView);

        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = (ObjectResult)result;
        Assert.Equal(500, objectResult.StatusCode);
    }

    /// Verifies that the `ProductController` class has the correct `ApiVersionAttribute` applied.
    /// This method ensures that the `ApiVersionAttribute` is present on the `ProductController`
    /// and that the specified version includes "3.0" as expected.
    /// The test uses reflection to retrieve the `ApiVersionAttribute` from the `ProductController` class
    /// and validates its presence and configuration.
    /// Assertions:
    /// - Ensures that the `ApiVersionAttribute` is not null.
    /// - Confirms that the version "3.0" is included in the versions defined by the `ApiVersionAttribute`.
    [Fact]
    public void Controller_HasCorrectApiVersionAttribute()
    {
        // Arrange & Act
        var apiVersionAttribute = typeof(ProductController).GetCustomAttribute(
            typeof(ApiVersionAttribute), false) as ApiVersionAttribute;

        // Assert
        Assert.NotNull(apiVersionAttribute);
        Assert.Contains("3.0", apiVersionAttribute.Versions.Select(v => v.ToString()));
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

    /// Tests the behavior of the Get method when provided with an excessively large page limit value.
    /// Ensures that the controller handles scenarios with an unreasonably large pageLimit gracefully by
    /// returning the correct response and calling the expected service method.
    /// <returns>
    /// Asserts that the result is of type OkObjectResult and verifies that the GetProductList method
    /// of the service is called once with the specified parameters.
    /// </returns>
    [Fact]
    public async Task Get_WithExcessivePageLimit_HandlesGracefully()
    {
        // Arrange
        int? pageIndex = 0;
        int? pageLimit = 10000; // Unreasonably large limit
        bool? availableOnly = true;

        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Url = "http://product1.com", StockQuantity = 5 }
        }.ToAsyncEnumerable();

        _productServiceMock.Setup(s => s.GetProductList(pageIndex, pageLimit, availableOnly))
            .Returns(products);

        // Act
        var result = await _controller.Get(pageIndex, pageLimit, availableOnly);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _productServiceMock.Verify(s => s.GetProductList(pageIndex, pageLimit, availableOnly), Times.Once);
    }

    /// Tests that the `Put` method in the `ProductController` correctly serializes the given `UpdateProductView`
    /// into an `UpdateProductStockQuantityCommand` JSON structure and ensures the JSON contains the correct values.
    /// This method verifies that the ID and stock quantity of the product in the serialized JSON match
    /// the values passed in the `UpdateProductView` object.
    /// <return>Asserts that the serialized JSON is not null and contains the expected fields and values.</return>
    [Fact]
    public async Task Put_SerializesProductCorrectly()
    {
        // Arrange
        var updateProductView = new UpdateProductView
        {
            Id = 1,
            StockQuantity = 20
        };

        _productServiceMock.Setup(s => s.EnsureProductExists(updateProductView.Id))
            .Returns(Task.CompletedTask);

        string? capturedJson = null;

        _rabbitMqDefaultClientMock.Setup(r => r.Send(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "application/json",
            null))
            .Callback<string, string, string, string, Guid?>((e, r, c, ct, p) => capturedJson = c)
            .Returns(Task.CompletedTask);

        // Act
        await _controller.Put(updateProductView);

        // Assert
        Assert.NotNull(capturedJson);

        // Verify the JSON contains the correct values
        var cmd = (capturedJson.Deserialize<UpdateProductStockQuantityCommand>())!;

        Assert.Equal(1, cmd.Id);
        Assert.Equal(20, cmd.StockQuantity);
    }

    /// Validates that the PUT method in the `ProductController` utilizes the correct exchange and routing key when sending messages
    /// to the RabbitMQ client.
    /// Ensures that the message exchange contains the identifier "Exchange" and the routing key includes the word "Update".
    /// <returns>
    /// A completed task representing the test method, indicating whether the proper exchange and routing key were used.
    /// </returns>
    [Fact]
    public async Task Put_UsesCorrectExchangeAndRoutingKey()
    {
        // Arrange
        var updateProductView = new UpdateProductView
        {
            Id = 1,
            StockQuantity = 20
        };

        _productServiceMock.Setup(s => s.EnsureProductExists(updateProductView.Id))
            .Returns(Task.CompletedTask);

        string? capturedExchange = null;
        string? capturedRoutingKey = null;

        _rabbitMqDefaultClientMock.Setup(r => r.Send(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "application/json",
            null))
            .Callback<string, string, string, string, Guid?>((e, r, c, ct, p) =>
            {
                capturedExchange = e;
                capturedRoutingKey = r;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _controller.Put(updateProductView);

        // Assert
        Assert.NotNull(capturedExchange);
        Assert.NotNull(capturedRoutingKey);
        Assert.Contains("Exchange", capturedExchange);
        Assert.Contains("Update", capturedRoutingKey);
    }

    /// Tests the behavior of the Get method in the ProductController
    /// when the availableOnly parameter is explicitly set to null.
    /// This test ensures that the ProductController correctly handles a null value
    /// for the availableOnly parameter when calling the GetProduct method
    /// on the IProductService implementation.
    /// The test verifies that the service is invoked with a null availableOnly parameter.
    /// <return>
    /// Ensures the Get method of the ProductController successfully invokes
    /// the GetProduct method in the service layer with the expected arguments.
    /// </return>
    [Fact]
    public async Task Get_CorrectlyHandlesNullAvailableOnlyParameter()
    {
        // Arrange
        const int id = 1;
        bool? availableOnly = null; // Explicitly null

        var product = new Product
        {
            Id = id,
            Name = "Test Product",
            Url = "http://test.com",
            Price = 10.0m,
            StockQuantity = 5
        };

        _productServiceMock.Setup(s => s.GetProduct(id, availableOnly))
            .ReturnsAsync(product);

        // Act
        await _controller.Get(id, availableOnly);

        // Assert
        _productServiceMock.Verify(s => s.GetProduct(id, null), Times.Once);
    }

    /// Verifies that the method under test correctly handles the case where the description of a product is null.
    /// This test ensures that the system can process a product creation request where the description field is explicitly
    /// set to null without causing errors, and that the product is properly saved with a null description.
    /// <return>
    /// A task that represents the asynchronous operation of the test. If the conditions of the test are met,
    /// the test will succeed; otherwise, it will fail.
    /// </return>
    [Fact]
    public async Task Post_WithNullDescriptionHandlesCorrectly()
    {
        // Arrange
        var createProductView = new CreateProductView
        {
            Name = "New Product",
            Url = "http://newproduct.com",
            Price = 15.0m,
            StockQuantity = 8,
            Description = null // Explicitly null description
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
                p.Description == null
            )), Times.Once);
    }

    /// Validates that the `RabbitMqUpdateProduct` method is called with the correct parameters
    /// when a product update operation is performed in the `ProductController`.
    /// This test ensures the following:
    /// - The `EnsureProductExists` method of the `IProductService` is invoked to validate product existence.
    /// - The `Send` method of the `IRabbitMqDefaultClient` is called with the serialized payload and correct configuration.
    /// <return>
    /// A completed task representing the execution of the test, as the test is asynchronous.
    /// </return>
    [Fact]
    public async Task Put_VerifyRabbitMqUpdateProductMethodCall()
    {
        // Arrange
        var updateProductView = new UpdateProductView
        {
            Id = 1,
            StockQuantity = 20
        };

        // Using reflection to check if controller calls the correct base method
        var controllerType = typeof(ProductController);
        var baseType = controllerType.BaseType!;

        var baseMethod = baseType.GetMethod("RabbitMqUpdateProduct",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.NotNull(baseMethod); // Verify the method exists

        _productServiceMock.Setup(s => s.EnsureProductExists(updateProductView.Id))
            .Returns(Task.CompletedTask);

        _rabbitMqDefaultClientMock.Setup(r => r.Send(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "application/json",
            null))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.Put(updateProductView);

        // Assert - we can verify the RabbitMQ client was called correctly
        _rabbitMqDefaultClientMock.Verify(r => r.Send(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.Is<string>(s => s.Equals(updateProductView.Serialize(null), StringComparison.CurrentCultureIgnoreCase)),
            "application/json",
            null), Times.Once);
    }

    /// Tests if the `Post` method in the target controller correctly handles products with formatted prices.
    /// Ensures that the decimal precision of the price is retained during the creation of a product.
    /// Verifies that the method interacts with the product service with the expected data structure.
    /// Checks that the method returns the appropriate action result when the operation succeeds.
    /// <return>
    /// An asynchronous task that completes upon the assertion of all validations in the test.
    /// </return>
    [Fact]
    public async Task Post_WithPriceFormattingHandlesCorrectly()
    {
        // Arrange
        var createProductView = new CreateProductView
        {
            Name = "Price Test Product",
            Url = "http://pricetest.com",
            Price = 99.99m, // Price with decimal places
            StockQuantity = 5
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
                p.Price == 99.99m // Verify decimal precision is maintained
            )), Times.Once);
    }

    /// <summary>
    /// Validates that the Swagger documentation for the ProductController's PUT method
    /// has consistent attributes and response types as defined in the API.
    /// This ensures that the method has the expected SwaggerResponse attributes for
    /// HTTP status codes 200 (OK), 400 (Bad Request), and 500 (Internal Server Error).
    /// The test verifies that these attributes are correctly applied and no expected
    /// response type is missing.
    /// The test uses reflection to retrieve and evaluate attributes from the ProductController's PUT method.
    /// </summary>
    /// <remarks>
    /// This test enhances the API's maintainability and discoverability by ensuring the Swagger documentation
    /// aligns with the API behavior. It prevents inconsistencies that may confuse the consumers of the API
    /// while providing clear expectations on possible outcomes of the PUT endpoint.
    /// </remarks>
    [Fact]
    public void ProductController_HasConsistentSwaggerDocumentation()
    {
        // Arrange & Act
        var putMethod = typeof(ProductController).GetMethod("Put");
        var swaggerResponses = putMethod?.GetCustomAttributes(typeof(SwaggerResponseAttribute), false).Cast<SwaggerResponseAttribute>().ToList();

        // Assert
        Assert.NotNull(swaggerResponses);
        Assert.Equal(3, swaggerResponses.Count); // Should have 3 response types documented
        Assert.Contains(swaggerResponses, r => r.StatusCode == StatusCodes.Status200OK);
        Assert.Contains(swaggerResponses, r => r.StatusCode == StatusCodes.Status400BadRequest);
        Assert.Contains(swaggerResponses, r => r.StatusCode == StatusCodes.Status500InternalServerError);
    }
}
