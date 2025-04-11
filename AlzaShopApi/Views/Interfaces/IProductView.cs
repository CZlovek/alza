namespace AlzaShopApi.Views.Interfaces;

/// <summary>
/// Defines the structure of a product view, combining multiple aspects
/// such as identifier, price, stock availability, and core product details.
/// </summary>
/// <remarks>
/// This interface inherits from several specialized interfaces, combining properties
/// related to the product's ID, pricing, stock quantity, and core attributes like
/// name, description, and URL.
/// </remarks>
public interface IProductView : IProductViewId, IProductViewPrice, IProductViewStockQuantity, IProductViewCore;