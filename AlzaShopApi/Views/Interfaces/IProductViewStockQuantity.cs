namespace AlzaShopApi.Views.Interfaces;

/// <summary>
/// Represents a contract that defines the stock quantity information for a product view.
/// </summary>
/// <remarks>
/// The IProductViewStockQuantity interface is intended to be used in scenarios
/// where the stock quantity of a product needs to be represented or modified.
/// It provides a property to get or set the available quantity of a product in stock.
/// </remarks>
public interface IProductViewStockQuantity
{
    /// <summary>
    /// Gets or sets the quantity of a product available in stock.
    /// </summary>
    /// <remarks>
    /// This property represents the number of items available for a particular product.
    /// If null, the stock quantity is unspecified or not applicable.
    /// </remarks>
    int? StockQuantity { get; set; }
}