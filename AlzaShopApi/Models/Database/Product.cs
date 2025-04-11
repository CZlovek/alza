using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlzaShopApi.Models.Database;

/// <summary>
/// Represents a product entity in the database.
/// </summary>
/// <remarks>
/// This class defines the core product data structure used throughout the application.
/// It maps directly to the "Products" table in the database.
/// </remarks>
[Table("Products")]
public class Product
{
    /// <summary>
    /// Gets the unique identifier for the product.
    /// </summary>
    /// <remarks>
    /// This property serves as the primary key in the database.
    /// Once initialized, it cannot be modified.
    /// </remarks>
    public int Id { get; init; }

    /// <summary>
    /// Gets the name of the product.
    /// </summary>
    /// <remarks>
    /// The name is required and cannot exceed 128 characters.
    /// Once initialized, it cannot be modified.
    /// </remarks>
    [MaxLength(128)] public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the URL associated with the product.
    /// </summary>
    /// <remarks>
    /// The URL is required and cannot exceed 512 characters.
    /// It typically represents a web address where the product can be viewed.
    /// </remarks>
    [MaxLength(512)] public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    /// <remarks>
    /// The price is represented as a decimal value.
    /// It defaults to 1 to avoid having products with zero or negative prices.
    /// </remarks>
    public decimal Price { get; set; } = 1;

    /// <summary>
    /// Gets or sets the description of the product.
    /// </summary>
    /// <remarks>
    /// The description is optional and cannot exceed 2048 characters.
    /// It provides detailed information about the product.
    /// </remarks>
    [MaxLength(2048)] public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the quantity of the product currently in stock.
    /// </summary>
    /// <remarks>
    /// This value represents the physical inventory count of the product.
    /// A value of zero or less indicates the product is out of stock.
    /// </remarks>
    public int StockQuantity { get; set; }
}
