namespace AlzaShopApi.Views.Interfaces;

/// <summary>
/// Represents a contract for managing the pricing details of a product.
/// </summary>
/// <remarks>
/// This interface provides an abstraction for handling the price of a product,
/// which may support nullable values to indicate the absence of pricing information.
/// It is intended to be implemented by product-related view models to unify pricing behavior.
/// </remarks>
public interface IProductViewPrice
{
    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    /// <remarks>
    /// The price represents the monetary value of the product. It can be a nullable decimal
    /// value to accommodate scenarios where the price is unknown or not applicable.
    /// </remarks>
    decimal? Price { get; set; }
}