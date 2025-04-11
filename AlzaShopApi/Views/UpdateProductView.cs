using AlzaShopApi.Views.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace AlzaShopApi.Views;

/// <summary>
/// Represents the view model used for updating a product.
/// Contains properties for identifying the product and updating its stock quantity.
/// </summary>
public class UpdateProductView : IUpdateProductView
{
    /// <summary>
    /// Gets or sets the unique identifier for a product.
    /// This property represents the primary key or unique ID used to identify a specific product
    /// within the system. It is required and must be provided when updating or identifying products.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Id cannot be zero or negative.")]
    public required int Id { get; set; }

    /// <summary>
    /// Represents the quantity of stock available for a product.
    /// This property is nullable and can be used to track the current stock level.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
    public required int? StockQuantity { get; set; }
}