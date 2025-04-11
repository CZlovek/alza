namespace AlzaShopApi.Views.Interfaces;

/// <summary>
/// Defines the core attributes of a product, including its name, URL, and an optional description.
/// </summary>
/// <remarks>
/// The IProductViewCore interface establishes a standardized contract for the essential properties
/// of a product, typically used in conjunction with other interfaces to represent more complex product views.
/// </remarks>
public interface IProductViewCore
{
    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    /// <remarks>
    /// This property represents the product's name as a required attribute.
    /// It is used in both product creation and display models to ensure the product's identity is clearly defined.
    /// </remarks>
    string Name { get; set; }

    /// <summary>
    /// Gets or sets the URL associated with the product.
    /// </summary>
    /// <remarks>
    /// This property represents the unique address or link to access the product information.
    /// It is required and serves as an identifier or navigational reference for the product.
    /// </remarks>
    string Url { get; set; }

    /// <summary>
    /// Gets or sets an optional description for the product.
    /// </summary>
    /// <remarks>
    /// This property provides additional details or context about the product.
    /// It is primarily used for display purposes and can include information
    /// to help users better understand the product's features or uses.
    /// </remarks>
    string? Description { get; set; }
}