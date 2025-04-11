namespace AlzaShopApi.Views.Interfaces;

/// <summary>
/// Represents an interface that defines the structure for identifying a product by its unique identifier.
/// </summary>
/// <remarks>
/// This interface is used as a base for other views or models requiring a product identification property.
/// It provides a single property to hold the unique identifier for a product.
/// </remarks>
public interface IProductViewId
{
    /// <summary>
    /// Gets or sets the unique identifier of a product.
    /// </summary>
    /// <remarks>
    /// This property represents a numeric ID used for identifying products across different views or operations.
    /// It is commonly utilized as a primary key in database operations or as a reference within APIs.
    /// </remarks>
    int Id { get; set; }
}