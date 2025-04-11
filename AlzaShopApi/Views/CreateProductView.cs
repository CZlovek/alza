using AlzaShopApi.Views.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace AlzaShopApi.Views;

/// <summary>
/// Represents a data structure used to create a new product in the system.
/// </summary>
/// <remarks>
/// The <see cref="CreateProductView"/> class provides properties to capture essential product details
/// such as name, URL, price, stock quantity, and an optional description.
/// It implements the <see cref="ICreateProductView"/> interface to ensure consistency with the required view structure.
/// </remarks>
public class CreateProductView : ICreateProductView
{
    /// <summary>
    /// Represents the price of a product.
    /// This value is optional and can be set to null if the price is not applicable or not determined.
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal? Price { get; set; }

    /// <summary>
    /// Represents the quantity of stock available for a product.
    /// This property defines the inventory level of the product, which can be used to track
    /// how many units are currently available for sale. A null value indicates that the stock
    /// information is not provided or applicable.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
    public int? StockQuantity { get; set; }

    /// <summary>
    /// Gets or sets the name of the product. This is a required property defining a product's unique identifier or title.
    /// </summary>
    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(128, ErrorMessage = "Name cannot be longer than 128 characters.")]
    public required string Name { get; set; }

    /// <summary>
    /// Represents the URL associated with the product.
    /// This is a required property that specifies the unique link or address
    /// to access additional information or details about the product.
    /// URL must conform to RFC 9110 section 15.5.1 standards.
    /// </summary>
    [Required(ErrorMessage = "Product URL is required.")]
    [Url(ErrorMessage = "The URL format is invalid. Please provide a valid URL according to RFC 9110.")]
    [StringLength(512, ErrorMessage = "URL cannot be longer than 512 characters.")]
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the description of the product.
    /// This provides additional information about the product.
    /// It is an optional property.
    /// </summary>
    [StringLength(2048, ErrorMessage = "Description cannot be longer than 2048 characters.")]
    public string? Description { get; set; }
}
