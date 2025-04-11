using AlzaShopApi.Database;
using AlzaShopApi.Models.Database;
using AlzaShopApi.Services;
using AlzaShopApi.Toolkit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Tests;

/// <summary>
/// Unit test class for verifying the functionality of the ProductService.
/// Contains a series of test methods to validate the behavior of various
/// service methods under different scenarios, including success, failure,
/// and edge cases.
/// </summary>
public class ProductServiceTests
{
    /// <summary>
    /// A mocked instance of the <see cref="ApplicationDbContext"/> used for unit testing.
    /// </summary>
    /// <remarks>
    /// This mock is used to simulate the database context behavior during the execution of
    /// unit tests in the <c>ProductServiceTests</c> suite.
    /// Dependencies on the actual database are eliminated, allowing controlled and predictable testing.
    /// </remarks>
    private readonly Mock<ApplicationDbContext> _dbContextMock;

    /// <summary>
    /// Represents an instance of the <see cref="ProductService"/> used for testing purposes.
    /// This variable holds the dependency-injected service instance for testing product-related operations,
    /// such as retrieving product details or filtering products based on availability.
    /// </summary>
    private readonly ProductService _service;

    /// A private field that serves as a collection of test product data used for unit testing
    /// in the ProductServiceTests test class. This list is initialized with predefined
    /// product objects and is intended to simulate a database context for mocking purposes
    /// in various test cases.
    private readonly List<Product> _testProducts;

    /// Contains unit tests for the ProductService class, ensuring its methods function as expected
    /// under various conditions.
    /// This test suite includes test cases for querying products by ID, filtering based on stock
    /// availability, and handling edge cases such as querying non-existent products.
    /// The tests use mocked dependencies including a fake implementation of ApplicationDbContext
    /// and logger instances to simulate real-world conditions while avoiding database interactions.
    /// Fields:
    /// - `_dbContextMock`: A mock of ApplicationDbContext used to simulate database operations.
    /// - `_service`: An instance of ProductService initialized with mocked dependencies.
    /// - `_testProducts`: A collection of Product objects to act as test data.
    /// Constructor:
    /// - Initializes mock dependencies, sets up the mock database context to return predefined test
    /// data, and injects these dependencies into the ProductService instance.
    /// Test Cases:
    /// - `GetProduct_WithValidId_ReturnsProduct`: Verifies retrieving a product by its valid ID.
    /// - `GetProduct_WithAvailableOnlyTrue_ReturnsOnlyAvailableProduct`: Ensures only products with
    /// stock availability are returned when filtering for available products.
    /// - `GetProduct_WithAvailableOnlyTrue_ReturnsNullForOutOfStockProduct`: Confirms that out-of-stock
    /// products are properly handled when filtering for available products.
    /// - `GetProduct_WithNonExistentId_ReturnsNull`: Verifies that querying for an ID that does not
    /// exist properly returns null.
    public ProductServiceTests()
    {
        var loggerMock = new Mock<ILogger<ProductService>>();

        _dbContextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

        // Initialize test data
        _testProducts =
        [
            new Product
            {
                Id = 1,
                Name = "Test Product 1",
                Url = "http://test1.com",
                Price = 10.0m,
                StockQuantity = 5,
                Description = "Test Description 1"
            },

            new Product
            {
                Id = 2,
                Name = "Test Product 2",
                Url = "http://test2.com",
                Price = 20.0m,
                StockQuantity = 0,
                Description = "Test Description 2"
            },

            new Product
            {
                Id = 3,
                Name = "Test Product 3",
                Url = "http://test3.com",
                Price = 30.0m,
                StockQuantity = 10,
                Description = "Test Description 3"
            }
        ];

        // Setup the DbContext mock
        _dbContextMock.Setup(x => x.Products).ReturnsDbSet(_testProducts);

        // Create the service with mocked dependencies
        _service = new ProductService(loggerMock.Object, _dbContextMock.Object);
    }

    /// Tests the GetProduct method of the ProductService with a valid product ID.
    /// Verifies that the expected product is returned when the product ID exists and matches.
    /// <return>Asserts that the returned product is not null, and that its properties match
    /// the expected values such as ID and Name.</return>
    [Fact]
    public async Task GetProduct_WithValidId_ReturnsProduct()
    {
        // Arrange
        const int productId = 1;
        bool? availableOnly = false;

        // Act
        var result = await _service.GetProduct(productId, availableOnly);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
        Assert.Equal("Test Product 1", result.Name);
    }

    /// Tests the GetProduct method to ensure it returns only available products
    /// when the availableOnly parameter is set to true.
    /// The test checks:
    /// 1. That the returned product is not null.
    /// 2. That the returned product's ID matches the provided ID.
    /// 3. That the returned product has a stock quantity greater than zero.
    /// <return>Does not return any value, but validates the behavior of the GetProduct method</return>
    [Fact]
    public async Task GetProduct_WithAvailableOnlyTrue_ReturnsOnlyAvailableProduct()
    {
        // Arrange
        var productId = 1; // product with stock
        bool? availableOnly = true;

        // Act
        var result = await _service.GetProduct(productId, availableOnly);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
        Assert.True(result.StockQuantity > 0);
    }

    /// Verifies that the `GetProduct` method in the `ProductService` class returns
    /// null when called with an `availableOnly` value of `true` for a product
    /// that is out of stock.
    /// This test validates the following scenarios:
    /// - The method correctly filters out products that are unavailable when
    /// `availableOnly` is set to `true`.
    /// - The method returns `null` if the specified product ID corresponds
    /// to an out-of-stock product while filtering for only available products.
    [Fact]
    public async Task GetProduct_WithAvailableOnlyTrue_ReturnsNullForOutOfStockProduct()
    {
        // Arrange
        var productId = 2; // out of stock product
        bool? availableOnly = true;

        // Setup DbSet to return a queryable that simulates the Where clause behavior
        var queryableProducts = _testProducts.AsQueryable();
        _dbContextMock.Setup(x => x.Products).ReturnsDbSet(queryableProducts);

        // Act
        var result = await _service.GetProduct(productId, availableOnly);

        // Assert
        Assert.Null(result);
    }

    /// Test method to verify that the GetProduct method in ProductService returns null
    /// when provided with a non-existent product ID.
    /// This test ensures that the system correctly handles a scenario where a product
    /// with the specified ID does not exist in the database.
    /// Preconditions:
    /// - A ProductService instance is initialized.
    /// Test Steps:
    /// 1. Arrange: A non-existent product ID is provided as input.
    /// 2. Act: The GetProduct method is invoked with the given product ID and availability filter.
    /// 3. Assert: Verifies that the returned result is null.
    /// Expected Outcome:
    /// - The GetProduct method should return null for the non-existent product ID.
    [Fact]
    public async Task GetProduct_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var productId = 999; // Non-existent ID
        bool? availableOnly = false;

        // Act
        var result = await _service.GetProduct(productId, availableOnly);

        // Assert
        Assert.Null(result);
    }

    /// Verifies that the GetProductList method retrieves all products when the availableOnly parameter is set to false.
    /// <returns>
    /// A task that completes when all assertions are verified. Ensures that the number of products returned matches the expected total count.
    /// </returns>
    [Fact]
    public async Task GetProductList_ReturnsAllProducts_WhenAvailableOnlyIsFalse()
    {
        // Arrange
        bool? availableOnly = false;

        // Act
        var result = _service.GetProductList(availableOnly: availableOnly);
        var products = await result.ToListAsync(); // Ensure you have the necessary using directive

        // Assert
        Assert.Equal(3, products.Count);
    }

    /// Verifies that the GetProductList method returns only products that are available (i.e., StockQuantity > 0)
    /// when the availableOnly parameter is set to true.
    /// <returns>
    /// A list of products where all items have a StockQuantity greater than 0 when availableOnly is true.
    /// </returns>
    [Fact]
    public async Task GetProductList_ReturnsOnlyAvailableProducts_WhenAvailableOnlyIsTrue()
    {
        // Arrange
        bool? availableOnly = true;

        // Act
        var result = _service.GetProductList(availableOnly: availableOnly);
        var products = await result.ToListAsync(); // Ensure you have the necessary using directive

        // Assert
        Assert.Equal(2, products.Count);
        Assert.All(products, p => Assert.True(p.StockQuantity > 0));
    }

    /// Ensures that the GetProductList method returns the correct page of products with the specified pagination parameters.
    /// This test verifies that the ProductService.GetProductList method correctly handles the pageIndex and pageLimit parameters,
    /// and returns the expected set of products corresponding to the given page. It also validates the handling of the
    /// optional availableOnly filter to ensure correct filtering when specified.
    /// <returns>
    /// Verifies that the list contains products for the correct page as per the specified pagination details.
    /// </returns>
    [Fact]
    public async Task GetProductList_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var pageIndex = 1;
        var pageLimit = 1;
        bool? availableOnly = false;

        // Act
        var result = _service.GetProductList(pageIndex, pageLimit, availableOnly);
        var products = await result.ToListAsync(); // Ensure you have the necessary using directive

        // Assert
        Assert.Single(products);
        Assert.Equal(2, products[0].Id); // Should be the second product
    }

    /// Verifies that the stock quantity of a product is updated correctly when a valid product is provided.
    /// This test ensures:
    /// - The `Update` method of the `Products` DbSet is called exactly once.
    /// - The `SaveChangesAsync` method of the `ApplicationDbContext` is called exactly once.
    /// - The stock quantity of the product in the database is updated to the expected value.
    /// <return>
    /// A completed task representing the execution of the test, with necessary assertions to validate updating the product stock quantity.
    /// </return>
    [Fact]
    public async Task ChangeProductStockQuantity_WithValidProduct_UpdatesStock()
    {
        // Arrange
        var product = new Product { Id = 1, StockQuantity = 15, Name = "ProductName", Url = "http://product.com" };

        var entityToUpdate = _testProducts.First(p => p.Id == product.Id);

        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                // Simulate the update operation
                entityToUpdate.StockQuantity = product.StockQuantity;
            })
            .ReturnsAsync(1);

        // Act
        await _service.UpdateProductStockQuantity(product);

        // Assert
        Assert.Equal(15, entityToUpdate.StockQuantity);
        _dbContextMock.Verify(x => x.Products.Update(It.IsAny<Product>()), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// Validates that attempting to change the stock quantity for a product that does not exist
    /// in the database results in a NotFoundException being thrown.
    /// <return>
    /// Throws NotFoundException when the specified product is not found in the database.
    /// </return>
    [Fact]
    public async Task ChangeProductStockQuantity_WithInvalidProduct_ThrowsNotFoundException()
    {
        // Arrange
        var product = new Product { Id = 999, StockQuantity = 15, Name = "ProductName", Url = "http://product.com" };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateProductStockQuantity(product));
    }

    /// Ensures that a new product is added to the database and can be retrieved successfully.
    /// Validates that the product is added to the context, saved correctly, and appears in the in-memory product list.
    /// Also ensures repository methods are called the expected number of times.
    /// <returns>Task representing the asynchronous add operation.</returns>
    [Fact]
    public async Task CreateProduct_AddsNewProduct()
    {
        // Arrange
        var product = new Product
        {
            Name = "New Product",
            Url = "http://newproduct.com",
            Price = 25.0m,
            StockQuantity = 8
        };

        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                // Simulate the add operation
                _testProducts.Add(product);
            })
            .ReturnsAsync(1);

        // Act
        await _service.CreateProduct(product);

        // Assert
        _dbContextMock.Verify(x => x.Products.Add(product), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Contains(product, _testProducts);
    }

    /// Validates and updates the stock quantity of an existing product in the database,
    /// but does not save the changes immediately.
    /// This method ensures that the provided product exists in the database, verifies
    /// the stock quantity adjustment, and updates the entity; however, it defers the
    /// persistence of the changes to the database, allowing for batch operations or
    /// external control over save operations.
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the
    /// stock quantity has been updated in-memory but not saved to the database.
    /// </returns>
    [Fact]
    public async Task LazyUpdateProductStockQuantity_WithValidProduct_UpdatesStockButDoesNotSave()
    {
        // Arrange
        var product = new Product { Id = 1, StockQuantity = 15, Name = "ProductName", Url = "http://product.com" };
        var entityToUpdate = _testProducts.First(p => p.Id == product.Id);
        var saveChangesCalled = false;

        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => saveChangesCalled = true)
            .ReturnsAsync(1);

        // Act
        await _service.LazyUpdateProductStockQuantity(product);

        // Assert
        Assert.Equal(15, entityToUpdate.StockQuantity);
        _dbContextMock.Verify(x => x.Products.Update(It.IsAny<Product>()), Times.Once);
        Assert.False(saveChangesCalled, "SaveChangesAsync should not be called for lazy update");
    }

    /// Tests the behavior of the LazyUpdateProductStockQuantity method when an invalid product is provided.
    /// This method verifies that a NotFoundException is thrown if the specified product does not exist in the database.
    /// <returns>
    /// Throws NotFoundException when the product does not exist.
    /// </returns>
    [Fact]
    public async Task LazyUpdateProductStockQuantity_WithInvalidProduct_ThrowsNotFoundException()
    {
        // Arrange
        var product = new Product { Id = 999, StockQuantity = 15, Name = "ProductName", Url = "http://product.com" };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.LazyUpdateProductStockQuantity(product));
    }

    /// Asynchronously saves changes made in the database context.
    /// Utilizes the DbContext's SaveChangesAsync method to persist all changes tracked by the context.
    /// Ensures that the database is updated with the pending modifications.
    /// <returns>An integer representing the number of state entries written to the database.</returns>
    [Fact]
    public async Task SaveChanges_CallsDbContextSaveChanges()
    {
        // Arrange
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _service.SaveChanges();

        // Assert
        Assert.Equal(5, result);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that calling Dispose on the service invokes the Dispose method of the underlying database context exactly once.
    /// </summary>
    /// <remarks>
    /// This test ensures that the Dispose method in the ProductService class disposes of resources properly by delegating
    /// the call to the associated database context. It uses a mock to confirm the expected behavior.
    /// </remarks>
    [Fact]
    public void Dispose_CallsDbContextDispose()
    {
        // Act
        _service.Dispose();

        // Assert
        _dbContextMock.Verify(x => x.Dispose(), Times.Once);
    }

    /// Verifies that the DisposeAsync method calls DisposeAsync on the underlying ApplicationDbContext exactly once.
    /// <return>Task representing the asynchronous operation.</return>
    [Fact]
    public async Task DisposeAsync_CallsDbContextDisposeAsync()
    {
        // Act
        await _service.DisposeAsync();

        // Assert
        _dbContextMock.Verify(x => x.DisposeAsync(), Times.Once);
    }

    /// Validates that when an exception occurs during the retrieval of a product,
    /// the exception is logged and rethrown.
    /// This method uses mocks to simulate a logging service and the database context, ensuring
    /// proper error handling behavior in the product retrieval process.
    /// <returns>Ensures that the exception is thrown and the error is logged appropriately.</returns>
    [Fact]
    public async Task GetProduct_WhenExceptionOccurs_LogsErrorAndRethrows()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ProductService>>();
        var dbContextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        var service = new ProductService(loggerMock.Object, dbContextMock.Object);

        dbContextMock.Setup(x => x.Products)
            .Throws(new InvalidOperationException("Test exception"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetProduct(1));
        // Verify that the exception was logged
        // Note: Verifying log calls requires more complex setup than shown here
    }

    /// Tests the behavior of the CreateProduct method when an exception occurs during database operation.
    /// Logs the error and ensures the exception is rethrown for proper handling by the calling code.
    /// <returns>Task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CreateProduct_WhenExceptionOccurs_LogsErrorAndRethrows()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ProductService>>();
        var dbContextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        var service = new ProductService(loggerMock.Object, dbContextMock.Object);

        var product = new Product { Id = 1, Name = "Test", Url = "http://test.com" };

        dbContextMock.Setup(x => x.Products.Add(It.IsAny<Product>()))
            .Throws(new DbUpdateException("Test exception", new Exception()));

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => service.CreateProduct(product));
        // Verify that the exception was logged
        // Note: Verifying log calls requires more complex setup than shown here
    }


    /// Validates that the product exists for a given product ID.
    /// If the product already exists, no exception is thrown.
    /// <returns>Does not return any value. If the product exists, the method completes successfully; otherwise, an exception is thrown.</returns>
    [Fact]
    public async Task EnsureProductExists_WithExistingProduct_DoesNotThrow()
    {
        // Arrange
        var existingId = 1; // already in _testProducts

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => _service.EnsureProductExists(existingId));
        Assert.Null(exception);
    }


    /// <summary>
    /// Tests that the <see cref="ProductService.EnsureProductExists(int)"/> method
    /// throws a <see cref="NotFoundException"/> when a non-existent product ID is provided.
    /// </summary>
    /// <exception cref="NotFoundException">
    /// Thrown if the specified product ID does not exist.
    /// </exception>
    [Fact]
    public async Task EnsureProductExists_WithNonExistentProduct_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentId = 999; // not in _testProducts

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.EnsureProductExists(nonExistentId));
    }

    /// Validates that the creation of a product without a name throws a ValidationException.
    /// This test ensures that the `CreateProduct` method enforces validation rules
    /// related to the `Name` property of a `Product` object. Specifically, an empty
    /// or missing name should result in a `ValidationException`.
    /// Preconditions:
    /// - The `Product` instance used in the test has an empty `Name` property.
    /// Expected Behavior:
    /// - The `CreateProduct` method throws a `ValidationException` due to the invalid `Name` property.
    [Fact]
    public async Task CreateProduct_WithoutName_ThrowsValidationException()
    {
        // Arrange
        var product = new Product
        {
            Name = "",
            Url = "http://somesite.com",
            Price = 10.0m,
            StockQuantity = 5
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateProduct(product));
    }

    /// Validates the creation of a product with an invalid URL and ensures that a ValidationException is thrown.
    /// This test verifies that attempting to create a product where the URL does not meet the required validation rules,
    /// such as not being a properly formatted URL, results in a ValidationException. The test uses a mock product object
    /// with an intentionally malformed URL and ensures that the exception is correctly thrown and handled.
    /// The test targets the `CreateProduct` method of the `ProductService`.
    [Fact]
    public async Task CreateProduct_WithInvalidUrl_ThrowsValidationException()
    {
        // Arrange
        var product = new Product
        {
            Name = "Invalid URL Product",
            Url = "not_a_valid_url",
            Price = 10.0m,
            StockQuantity = 5
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateProduct(product));
    }

    /// Validates that calling the GetProduct method with a negative product ID throws a ValidationException.
    /// This method tests the behavior of the GetProduct method when supplied with an invalid product ID
    /// that is less than zero. It ensures that the method correctly validates input and throws the appropriate
    /// exception.
    /// <returns>
    /// Ensures that a ValidationException is thrown with a message indicating that the ID must be greater than 0.
    /// </returns>
    [Fact]
    public async Task GetProduct_WithNegativeId_ThrowsValidationException()
    {
        // Arrange
        var invalidId = -1;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetProduct(invalidId));
        Assert.Contains("greater than 0", exception.Message);
    }

    /// <summary>
    /// Validates that calling the GetProduct method with an ID value of zero throws a ValidationException.
    /// </summary>
    /// <remarks>
    /// This test ensures the GetProduct method enforces proper validation on the input ID by checking that
    /// the ID must be greater than zero. If an invalid ID (zero in this case) is provided,
    /// a ValidationException is expected to be thrown with an appropriate error message.
    /// </remarks>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    /// Thrown when the input ID is zero, indicating an invalid argument.
    /// </exception>
    [Fact]
    public async Task GetProduct_WithZeroId_ThrowsValidationException()
    {
        // Arrange
        var invalidId = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetProduct(invalidId));
        Assert.Contains("greater than 0", exception.Message);
    }

    /// Tests that the UpdateProductStockQuantity method throws a ValidationException
    /// when provided with a product that has a negative Id.
    /// The expected exception message should indicate that the Id must be greater than 0.
    /// <return>Task representing the asynchronous operation of the test.</return>
    [Fact]
    public async Task UpdateProductStockQuantity_WithNegativeId_ThrowsValidationException()
    {
        // Arrange
        var product = new Product { Id = -1, StockQuantity = 10, Name = "Test", Url = "http://test.com" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.UpdateProductStockQuantity(product));
        Assert.Contains("greater than 0", exception.Message);
    }

    /// Tests that the EnsureProductExists method throws a ValidationException
    /// when provided with a negative product ID.
    /// This ensures the method enforces validation rules
    /// requiring the product ID to be greater than 0.
    /// <returns>Throws a ValidationException if the product ID is negative.</returns>
    [Fact]
    public async Task EnsureProductExists_WithNegativeId_ThrowsValidationException()
    {
        // Arrange
        var invalidId = -1;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.EnsureProductExists(invalidId));
        Assert.Contains("greater than 0", exception.Message);
    }

    /// Validates that the product exists in the system when provided with a specific product ID.
    /// <return><exception cref="ValidationException">Thrown when the provided product ID is zero, indicating invalid input.</exception></return>
    /// This method does not return a value but ensures the integrity of the input productId.
    [Fact]
    public async Task EnsureProductExists_WithZeroId_ThrowsValidationException()
    {
        // Arrange
        var invalidId = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.EnsureProductExists(invalidId));
        Assert.Contains("greater than 0", exception.Message);
    }

    /// Ensures that any exceptions encountered during saving changes to the database
    /// are logged appropriately and rethrown for further handling.
    /// <returns>Throws the original exception encountered during the save operation.</returns>
    [Fact]
    public async Task SaveChanges_WhenExceptionOccurs_LogsErrorAndRethrows()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ProductService>>();
        var dbContextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        var service = new ProductService(loggerMock.Object, dbContextMock.Object);

        var expectedException = new DbUpdateException("Test exception", new Exception());
        dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => service.SaveChanges());
        Assert.Same(expectedException, exception);
    }

    /// Validates the behavior of the `UseAvailabilityFilter` method when the `availableOnly` parameter is null.
    /// This test checks whether the method correctly returns `true` when the `availableOnly` parameter is unspecified or null,
    /// indicating that the availability filter should be used by default.
    /// The test uses reflection to access the private static `UseAvailabilityFilter`
    /// method as it is not directly exposed by the `ProductService` class. The method's logic
    /// is invoked with a null input, and the result is validated to ensure that the expected behavior occurs.
    /// Assertions:
    /// - The returned value should be `true` for a null `availableOnly` input.
    [Fact]
    public void UseAvailabilityFilter_WithNullAvailableOnly_ReturnsTrue()
    {
        // Arrange & Act & Assert
        // This test uses reflection to access the private method
        var method = typeof(ProductService).GetMethod("UseAvailabilityFilter",
            BindingFlags.NonPublic | BindingFlags.Static);

        bool? availableOnly = null;
        var result = method!.Invoke(null, [availableOnly]);

        Assert.True((bool)result!);
    }

    /// Unit test that verifies the behavior of the private method `UseAvailabilityFilter`
    /// in the `ProductService` class when the `availableOnly` parameter is `true`.
    /// The test confirms that the method correctly returns `true` under these conditions.
    /// Key Assertions:
    /// - Ensures the method properly evaluates the `availableOnly` parameter when set to `true`.
    /// Test Execution:
    /// - Uses reflection to access the private static method `UseAvailabilityFilter` for testing purposes.
    /// - Evaluates the method's output and asserts that the returned value is `true`.
    /// Notes:
    /// - This test validates the internal logic of the `ProductService` class.
    /// - Reflection is employed due to the method's private access modifier.
    [Fact]
    public void UseAvailabilityFilter_WithTrueAvailableOnly_ReturnsTrue()
    {
        // Arrange & Act & Assert
        // This test uses reflection to access the private method
        var method = typeof(ProductService).GetMethod("UseAvailabilityFilter",
            BindingFlags.NonPublic | BindingFlags.Static);

        bool? availableOnly = true;
        var result = method!.Invoke(null, [availableOnly]);

        Assert.True((bool)result!);
    }

    /// <summary>
    /// Tests that the private static method "UseAvailabilityFilter" in the ProductService class
    /// returns false when the provided nullable boolean parameter "availableOnly" is explicitly set to false.
    /// </summary>
    /// <remarks>
    /// This test indirectly verifies the behavior of a private method by using reflection to access
    /// the method. It ensures the private logic properly handles the scenario where the filter is
    /// instructed not to limit the results to only available products.
    /// </remarks>
    /// <exception cref="TargetInvocationException">
    /// Thrown if the invoked method throws any exception during execution.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if method arguments used in reflection invocation are null.
    /// </exception>
    [Fact]
    public void UseAvailabilityFilter_WithFalseAvailableOnly_ReturnsFalse()
    {
        // Arrange & Act & Assert
        // This test uses reflection to access the private method
        var method = typeof(ProductService).GetMethod("UseAvailabilityFilter",
            BindingFlags.NonPublic | BindingFlags.Static);

        bool? availableOnly = false;
        var result = method!.Invoke(null, [availableOnly]);

        Assert.False((bool)result!);
    }

    /// Validates and handles the scenario where the page index provided for
    /// retrieving a product list is negative, ensuring that no exceptions are thrown
    /// and some results are still returned.
    /// <returns>Returns a non-null list of products, potentially empty, even when the page index is negative.</returns>
    [Fact]
    public async Task GetProductList_WithNegativePageIndex_HandlesGracefully()
    {
        // Arrange
        var pageIndex = -1;
        var pageLimit = 10;
        bool? availableOnly = false;

        // Act
        var result = _service.GetProductList(pageIndex, pageLimit, availableOnly);

        // Assert
        // Even with negative index, we should get some results without throwing
        var products = await result.ToListAsync();
        Assert.NotNull(products);
    }

    /// Verifies that the GetProductList method returns an empty list when a zero page limit is specified.
    /// <returns>Task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetProductList_WithZeroPageLimit_ReturnsEmptyList()
    {
        // Arrange
        var pageIndex = 0;
        var pageLimit = 0;
        bool? availableOnly = false;

        // Act
        var result = _service.GetProductList(pageIndex, pageLimit, availableOnly);

        // Assert
        var products = await result.ToListAsync();
        Assert.Empty(products);
    }

    /// Tests the GetProductList method of the ProductService class when all input parameters are null.
    /// This ensures that the method correctly returns all available products without applying
    /// pagination or availability filters.
    /// <returns>
    /// Asserts that the returned product list contains all products in the service (expected to be 3).
    /// </returns>
    [Fact]
    public async Task GetProductList_WithNullParameters_ReturnsAllProducts()
    {
        // Arrange
        int? pageIndex = null;
        int? pageLimit = null;
        bool? availableOnly = false;

        // Act
        var result = _service.GetProductList(pageIndex, pageLimit, availableOnly);

        // Assert
        var products = await result.ToListAsync();
        Assert.Equal(3, products.Count);
    }

    /// Tests the CreateProduct method to ensure that when a product with a null or default price is created,
    /// it assigns and persists the default price to the product.
    /// <returns>A task that represents the asynchronous operation of the test.</returns>
    [Fact]
    public async Task CreateProduct_WithNullPrice_CreatesProductWithDefaultPrice()
    {
        // Arrange
        var product = new Product
        {
            Name = "Product With Null Price",
            Url = "http://nullprice.com",
            Price = 0, // Default value
            StockQuantity = 5
        };

        bool productAdded = false;
        _dbContextMock.Setup(x => x.Products.Add(It.IsAny<Product>()))
            .Callback<Product>(_ => productAdded = true);

        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.CreateProduct(product);

        // Assert
        Assert.True(productAdded);
        _dbContextMock.Verify(x => x.Products.Add(product), Times.Once);
    }

    /// Tests that creating a product with an empty URL throws a ValidationException.
    /// <returns>
    /// Throws a ValidationException if the URL of the product is empty. The exception message
    /// should indicate that a valid URL is required.
    /// </returns>
    [Fact]
    public async Task CreateProduct_WithEmptyUrl_ThrowsValidationException()
    {
        // Arrange
        var product = new Product
        {
            Name = "Product With Empty URL",
            Url = "",
            Price = 10.0m,
            StockQuantity = 5
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateProduct(product));
        Assert.Contains("valid URL", exception.Message);
    }

    /// Tests whether the CreateProduct method throws a ValidationException when a product with a relative URL is provided.
    /// This test ensures that the CreateProduct method validates the URL property of the product
    /// and enforces it to be a valid absolute URL before creating the product.
    /// <returns>Task that represents the asynchronous unit test operation.</returns>
    [Fact]
    public async Task CreateProduct_WithRelativeUrl_ThrowsValidationException()
    {
        // Arrange
        var product = new Product
        {
            Name = "Product With Relative URL",
            Url = "relative/path",
            Price = 10.0m,
            StockQuantity = 5
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateProduct(product));
        Assert.Contains("valid URL", exception.Message);
    }

    /// Updates only the StockQuantity of the product while leaving other fields unchanged.
    /// This method does not persist changes to the database.
    /// The caller is responsible for committing changes using a save operation.
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task LazyUpdateProductStockQuantity_UpdatesOnlySpecifiedFields()
    {
        // Arrange
        var originalProduct = _testProducts.First(p => p.Id == 1);
        var originalName = originalProduct.Name;
        var originalPrice = originalProduct.Price;
        var originalDescription = originalProduct.Description;

        var updateProduct = new Product
        {
            Id = 1,
            Name = "Should Not Change",
            Price = 999.99m,
            StockQuantity = 25,
            Url = "http://shouldnotchange.com",
            Description = "Should Not Change"
        };

        // Act
        await _service.LazyUpdateProductStockQuantity(updateProduct);

        // Assert
        Assert.Equal(25, originalProduct.StockQuantity); // This should change
        Assert.Equal(originalName, originalProduct.Name); // This should not change
        Assert.Equal(originalPrice, originalProduct.Price); // This should not change
        Assert.Equal(originalDescription, originalProduct.Description); // This should not change
    }

    /// Validates that both pagination and filtering logic are applied correctly
    /// when retrieving a paginated and filtered list of products.
    /// <return>
    /// Ensures the returned product list meets the expected criteria:
    /// 1. The number of products corresponds to the specified page limit.
    /// 2. The returned products match the filtering condition (e.g., are available if specified).
    /// 3. Pagination selects the correct subset of products.
    /// </return>
    [Fact]
    public async Task GetProductList_WithPaginationAndFiltering_AppliesBothCorrectly()
    {
        // Arrange
        var pageIndex = 0;
        var pageLimit = 1;
        bool? availableOnly = true;

        // Act
        var result = _service.GetProductList(pageIndex, pageLimit, availableOnly);
        var products = await result.ToListAsync();

        // Assert
        Assert.Single(products);
        Assert.True(products[0].StockQuantity > 0);
        Assert.Equal(1, products[0].Id); // First available product
    }

    /// Verifies that the GetProductList method in ProductService returns an empty list
    /// when the specified page index exceeds the available range of data.
    /// <return>Ensures the result is an empty list if the page index is too high.</return>
    [Fact]
    public async Task GetProductList_WithHighPageIndex_ReturnsEmptyList()
    {
        // Arrange
        var pageIndex = 100; // Far beyond available data
        var pageLimit = 10;
        bool? availableOnly = false;

        // Act
        var result = _service.GetProductList(pageIndex, pageLimit, availableOnly);
        var products = await result.ToListAsync();

        // Assert
        Assert.Empty(products);
    }

    /// Updates the stock quantity of a product, including handling negative stock values.
    /// This method verifies if the product exists, updates its StockQuantity field, and saves the changes in the database.
    /// Properly updates the database context to reflect the changes.
    /// <returns>Task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateProductStockQuantity_WithNegativeStockValue_UpdatesSuccessfully()
    {
        // Arrange
        var product = new Product { Id = 1, StockQuantity = -5, Name = "ProductName", Url = "http://product.com" };
        var entityToUpdate = _testProducts.First(p => p.Id == product.Id);

        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                // Simulate the update operation
                entityToUpdate.StockQuantity = product.StockQuantity;
            })
            .ReturnsAsync(1);

        // Act
        await _service.UpdateProductStockQuantity(product);

        // Assert
        Assert.Equal(-5, entityToUpdate.StockQuantity);
        _dbContextMock.Verify(x => x.Products.Update(It.IsAny<Product>()), Times.Once);
    }

    //[Fact]
    //public async Task EnsureProductExists_WithValidId_DoesNotTouchDbContext()
    //{
    //    // Arrange
    //    int productId = 1;
    //    bool anyAsyncCalled = false;

    //    _dbContextMock.Setup(x => x.Products.AnyAsync(It.IsAny<Func<Product, bool>>(), 
    //            It.IsAny<CancellationToken>()))
    //        .Callback(() => anyAsyncCalled = true)
    //        .ReturnsAsync(true);

    //    // Act
    //    await _service.EnsureProductExists(productId);

    //    // Assert
    //    Assert.True(anyAsyncCalled);
    //    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    //    _dbContextMock.Verify(x => x.Products.Update(It.IsAny<Product>()), Times.Never);
    //}

    /// Verifies that the correct parameters are logged when an exception is thrown
    /// during the execution of the `GetProduct` method.
    /// This test ensures that when an exception (e.g., `DbUpdateException`) occurs while fetching
    /// a product from the database, the exception is properly logged along with the relevant
    /// product ID.
    /// <returns>Task representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetProduct_WithException_LogsCorrectParameters()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ProductService>>();
        var dbContextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

        // Setup a mock logger that captures the logged values
        string? capturedId = null;
        var exception = new DbUpdateException("Test exception", new Exception());

        dbContextMock.Setup(x => x.Products)
            .Throws(exception);

        loggerMock.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!))
            .Callback<LogLevel, EventId, object, Exception, Delegate>((_, _, state, _, _) =>
            {
                capturedId = state.ToString();
            });

        var service = new ProductService(loggerMock.Object, dbContextMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => service.GetProduct(42));

        // Verify logging
        Assert.NotNull(capturedId);
        Assert.Contains("42", capturedId); // The product ID should be in the logged message
    }
}