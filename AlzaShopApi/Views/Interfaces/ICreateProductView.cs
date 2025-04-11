namespace AlzaShopApi.Views.Interfaces;

/// <summary>
/// Defines the view used for creating a product, providing required properties for product details.
/// </summary>
/// <remarks>
/// The ICreateProductView interface aggregates the core product information,
/// pricing details, and stock quantity, ensuring a unified contract for creating new products.
/// </remarks>
public interface ICreateProductView : IProductViewPrice, IProductViewStockQuantity, IProductViewCore;