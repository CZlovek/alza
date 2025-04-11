namespace AlzaShopApi.Models;

/// <summary>
/// A command that encapsulates the operation of updating the stock quantity for a specific product.
/// </summary>
/// <remarks>
/// This command carries the necessary data to update the stock level of a product in the system.
/// It is typically used in conjunction with a message broker to process the operation asynchronously.
/// This class is intended to be immutable and follows the record type pattern for consistency and safety
/// in concurrent operations.
/// </remarks>
public record UpdateProductStockQuantityCommand : ICommand
{
    /// <summary>
    /// Gets the unique identifier of the product associated with the command.
    /// </summary>
    /// <remarks>
    /// This property is used within the command to determine which specific product
    /// the operation should be performed on, such as updating its stock quantity.
    /// The value must be a valid integer representing the product's ID.
    /// </remarks>
    public int Id { get; init; }

    /// <summary>
    /// Represents the stock quantity of a product.
    /// </summary>
    /// <remarks>
    /// The property is used to indicate the number of items available in stock for a specific product.
    /// It is primarily utilized in commands and operations to update the inventory level of a product.
    /// Changes to this property typically trigger updates in the database or other related services.
    /// </remarks>
    public int StockQuantity { get; init; }
}