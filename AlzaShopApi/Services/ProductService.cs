using System.ComponentModel.DataAnnotations;
using AlzaShopApi.Database;
using AlzaShopApi.Models.Database;
using AlzaShopApi.Services.Interfaces;
using AlzaShopApi.Toolkit;
using Microsoft.EntityFrameworkCore;

namespace AlzaShopApi.Services;

/// <summary>
/// Provides functionalities for managing product-related data and operations.
/// </summary>
/// <remarks>
/// This class implements <see cref="IProductService"/> and facilitates product retrieval, creation,
/// updates, and validation. It includes mechanisms for managing inventory and saving changes to the database.
/// Additionally, it implements <see cref="IAsyncDisposable"/> for efficient resource management in asynchronous operations.
/// </remarks>
public class ProductService : IProductService, IAsyncDisposable
{
    /// <summary>
    /// Serves as the database context for managing and performing operations on application data.
    /// </summary>
    /// <remarks>
    /// Facilitates interactions with the database using the <see cref="ApplicationDbContext"/> class.
    /// Commonly utilized for operations such as creating, reading, updating, and deleting product-related entities.
    /// This field is primarily leveraged in the service layer of the application.
    /// </remarks>
    private readonly ApplicationDbContext _applicationDbContext;

    /// <summary>
    /// Instance of <see cref="ILogger{TCategoryName}"/> used for logging operations and events within the service.
    /// </summary>
    /// <remarks>
    /// Enables structured logging of informational messages, warnings, and errors in the <see cref="ProductService"/>.
    /// Facilitates monitoring and troubleshooting by capturing details of key operations and exceptions during execution.
    /// </remarks>
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Provides functionality for managing, retrieving, and processing product data from the database.
    /// </summary>
    public ProductService(ILogger<ProductService> logger, ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product to retrieve.</param>
    /// <param name="availableOnly">
    /// Determines whether to filter the product based on availability.
    /// If true, only products with stock greater than 0 will be returned.
    /// If false, the product will be returned regardless of stock quantity.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The result contains the product if found, otherwise null if the product does not exist
    /// or does not match the specified filtering criteria.
    /// </returns>
    /// <exception cref="Exception">Thrown if an operational error occurs during processing.</exception>
    public async Task<Product?> GetProduct(int id, bool? availableOnly = null)
    {
        try
        {
            if (id < 1)
            {
                throw new ValidationException("Id must be greater than 0.");
            }

            var query = UseAvailabilityFilter(availableOnly)
                ? _applicationDbContext.Products.Where(k => k.Id == id && k.StockQuantity > 0)
                : _applicationDbContext.Products.Where(k => k.Id == id);

            return await query.FirstOrDefaultAsync();
        }

        catch (Exception error)
        {
            _logger.LogError(error, "Error getting product: {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Determines whether the availability filter should be applied based on the provided parameter.
    /// </summary>
    /// <param name="availableOnly">A nullable boolean indicating if only available products should be considered. If null or true, the filter is applied.</param>
    /// <returns>A boolean value indicating whether the availability filter is applicable.</returns>
    private static bool UseAvailabilityFilter(bool? availableOnly)
    {
        return availableOnly == null || availableOnly.Value;
    }

    /// <summary>
    /// Retrieves a paginated and optionally filtered list of products.
    /// </summary>
    /// <param name="pageIndex">
    /// The zero-based index of the page for pagination. Determines where the product list starts if used with pageLimit.
    /// </param>
    /// <param name="pageLimit">
    /// The maximum number of products to return per page when paginating.
    /// </param>
    /// <param name="availableOnly">
    /// Specifies whether to filter products based on availability. If true or null, only products with stock greater than 0 are returned; if false, all products are included.
    /// </param>
    /// <returns>
    /// An asynchronous enumerable collection of <see cref="Product"/> instances that meet the specified criteria.
    /// </returns>
    public IAsyncEnumerable<Product> GetProductList(int? pageIndex = null, int? pageLimit = null,
        bool? availableOnly = null)
    {
        var products = UseAvailabilityFilter(availableOnly)
            ? _applicationDbContext.Products.Where(k => k.StockQuantity > 0)
            : _applicationDbContext.Products;

        return pageIndex != null && pageLimit != null
            ? products.Skip(pageIndex.Value * pageLimit.Value).Take(pageLimit.Value).AsAsyncEnumerable()
            : products.AsAsyncEnumerable();
    }

    /// <summary>
    /// Updates the stock quantity of a specific product in the database.
    /// </summary>
    /// <param name="product">The product object containing its ID and updated stock quantity. Only its ID and stock quantity are utilized in the operation.</param>
    /// <returns>A task representing the asynchronous operation of updating the product's stock quantity.</returns>
    /// <exception cref="NotFoundException">Thrown when the product is not found in the database.</exception>
    /// <exception cref="Exception">Thrown for general errors encountered during the update operation.</exception>
    public async Task UpdateProductStockQuantity(Product product)
    {
        try
        {
            if (product.Id < 1)
            {
                throw new ValidationException("Id must be greater than 0.");
            }

            var entity = await GetProduct(product.Id, false);

            if (entity != null)
            {
                entity.StockQuantity = product.StockQuantity;

                _applicationDbContext.Products.Update(entity);
                await _applicationDbContext.SaveChangesAsync();
            }
            else
            {
                throw new NotFoundException(product.Id);
            }
        }

        catch (Exception error)
        {
            _logger.LogError(error, "Error changing product stock quantity: {Id}", product.Id);
            throw;
        }
    }


    /// <summary>
    /// Updates the stock quantity of a given product asynchronously without committing changes to the database.
    /// </summary>
    /// <param name="product">The product instance containing the updated stock quantity and associated ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LazyUpdateProductStockQuantity(Product product)
    {
        try
        {
            var entity = await GetProduct(product.Id, false);

            if (entity != null)
            {
                entity.StockQuantity = product.StockQuantity;

                _applicationDbContext.Products.Update(entity);
            }
            else
            {
                throw new NotFoundException(product.Id);
            }
        }

        catch (Exception error)
        {
            _logger.LogError(error, "Error changing product stock quantity: {Id}", product.Id);
            throw;
        }
    }

    /// <summary>
    /// Saves all changes made in the application context to the database.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the number of
    /// state entries written to the database.
    /// </returns>
    public async Task<int> SaveChanges()
    {
        try
        {
            return await _applicationDbContext.SaveChangesAsync();
        }

        catch (Exception error)
        {
            _logger.LogError(error, "Error saving changes to the database.");
            throw;
        }
    }

    /// <summary>
    /// Creates a new product in the database.
    /// </summary>
    /// <param name="product">The product to be created, containing all necessary details like name, price, and stock quantity.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="Exception">Throws an exception if an error occurs during product creation.</exception>
    public async Task CreateProduct(Product product)
    {
        try
        {
            if (string.IsNullOrEmpty(product.Name))
            {
                throw new ValidationException("Name is required.");
            }

            if (string.IsNullOrEmpty(product.Url) || !Uri.TryCreate(product.Url, UriKind.Absolute, out _))
            {
                throw new ValidationException("Url is required and must be a valid URL.");
            }

            _applicationDbContext.Products.Add(product);

            await _applicationDbContext.SaveChangesAsync();
        }

        catch (Exception error)
        {
            _logger.LogError(error, "Error creating product: {Name} | {Url}", product.Name, product.Url);
            throw;
        }
    }

    /// <summary>
    /// Ensures that a product with the specified ID exists in the database.
    /// </summary>
    /// <param name="productId">The unique identifier of the product to verify.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <exception cref="NotFoundException">Thrown when the product with the specified ID does not exist.</exception>
    public async Task EnsureProductExists(int productId)
    {
        if (productId < 1)
        {
            throw new ValidationException("Id must be greater than 0.");
        }

        if (!await _applicationDbContext.Products.AnyAsync(k => k.Id == productId))
        {
            throw new NotFoundException(productId);
        }
    }

    /// <summary>
    /// Releases the resources used by the <see cref="ProductService"/> instance, including unmanaged and managed resources.
    /// </summary>
    /// <remarks>
    /// This method ensures that the internal database context is properly disposed to free up system resources.
    /// After calling this method, the instance of <see cref="ProductService"/> should not be used further.
    /// </remarks>
    public void Dispose()
    {
        _applicationDbContext.Dispose();
    }

    /// <summary>
    /// Asynchronously disposes the resources held by the implementing class, ensuring proper resource cleanup.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await _applicationDbContext.DisposeAsync();
    }
}
