using AlzaShopApi.Models.Database;
using AlzaShopApi.Toolkit;

namespace AlzaShopApi.Services.Interfaces;

/// <summary>
/// Represents the service interface for managing product-related operations.
/// </summary>
/// <remarks>
/// This interface defines methods for handling operations such as fetching product details,
/// managing product stock, and persisting changes, ensuring a separation of concerns
/// in the application architecture.
/// </remarks>
public interface IProductService : IDisposable
{
    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product to retrieve.</param>
    /// <param name="availableOnly">
    /// Indicates whether to only return products with stock quantity greater than 0.
    /// If true (default), only products with available stock are returned.
    /// If false or null, the product is returned regardless of stock quantity.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the product if found; otherwise, null.
    /// </returns>
    Task<Product?> GetProduct(int id, bool? availableOnly = true);

    /// <summary>
    /// Retrieves a filtered and paginated list of products.
    /// </summary>
    /// <param name="pageIndex">
    /// The zero-based page index for pagination. If specified with pageLimit, determines which page of results to return.
    /// If specified without pageLimit, skips the specified number of results.
    /// </param>
    /// <param name="pageLimit">
    /// The maximum number of products to return. If specified with pageIndex, determines the page size.
    /// If specified without pageIndex, returns the first specified number of results.
    /// </param>
    /// <param name="availableOnly">
    /// When true or null, only returns products with stock quantity greater than 0.
    /// When false, returns all products regardless of stock quantity.
    /// </param>
    /// <returns>
    /// An asynchronous enumerable collection of products matching the specified criteria.
    /// </returns>
    IAsyncEnumerable<Product> GetProductList(int? pageIndex = null, int? pageLimit = null, bool? availableOnly = null);

    /// <summary>
    /// Updates the stock quantity of an existing product.
    /// </summary>
    /// <param name="product">
    /// The product containing its unique identifier (ID) and the updated stock quantity.
    /// Only the ID and StockQuantity properties will be processed.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// Thrown when a product with the specified identifier does not exist in the database.
    /// </exception>
    Task UpdateProductStockQuantity(Product product);

    /// <summary>
    /// Creates a new product in the system.
    /// </summary>
    /// <param name="product">
    /// The product to create. Must include all required fields such as Name and URL.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task CreateProduct(Product product);

    /// <summary>
    /// Ensures that a product with the given ID exists in the database.
    /// </summary>
    /// <param name="productId">The unique identifier of the product to verify its existence.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// If the product does not exist, an exception might be thrown or handled depending on the implementation.
    /// </returns>
    Task EnsureProductExists(int productId);


    /// <summary>
    /// Performs a lazy update of the stock quantity for a given product.
    /// </summary>
    /// <param name="product">
    /// The product object containing the updated stock quantity to be applied.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation of updating the stock quantity.
    /// </returns>
    Task LazyUpdateProductStockQuantity(Product product);

    /// <summary>
    /// Persists all changes made in the context to the database.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous save operation.
    /// The task result contains the number of state entries written to the database.
    /// </returns>
    Task<int> SaveChanges();
}
