using AlzaShopApi.Views.Interfaces;

namespace AlzaShopApi.Views;

/// <summary>
/// Represents a product view model used for displaying product information.
/// </summary>
/// <remarks>
/// This class provides the structure for displaying product details, including
/// properties such as identifier, price, stock availability, and additional
/// descriptive information.
/// </remarks>
public class ProductView : IProductView
{
    /// <summary>
    /// Gets or sets the unique identifier of the product.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the price of the product.
    /// This value is nullable, indicating that a price may not always be available.
    /// </summary>
    public decimal? Price { get; set; }

    /// Represents the quantity of a product available in stock.
    /// This value indicates the inventory level for a product.
    /// It is nullable, meaning it can have a value or be null to represent an undefined stock status.
    public int? StockQuantity { get; set; }

    /// Gets or sets the name of the product.
    /// This property is required and represents the identifiable name
    /// of a product in the product view model.
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the URL related to the product.
    /// </summary>
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the description of the product.
    /// </summary>
    public string? Description { get; set; }
}