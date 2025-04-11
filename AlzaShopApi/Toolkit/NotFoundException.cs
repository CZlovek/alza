namespace AlzaShopApi.Toolkit;

/// <summary>
/// Represents an exception that is thrown when a requested product is not found in the system.
/// </summary>
/// <remarks>
/// This exception is typically used to indicate that a specific product, identified by its unique product ID,
/// could not be located in the data source or database. It provides the product ID within the exception message
/// for easier debugging and identification of the missing resource.
/// </remarks>
/// <exception cref="System.Exception">Inherits from the base <see cref="System.Exception"/> class.</exception>
/// <param name="productId">The unique identifier of the product that was not found.</param>
public class NotFoundException(int productId) : Exception($"Product {productId} not found")
{
    /// <summary>
    /// Represents the unique identifier of a product that could not be found.
    /// This variable is used to provide the product ID associated with
    /// a NotFoundException in the application.
    /// </summary>
    public readonly int ProductId = productId;
}