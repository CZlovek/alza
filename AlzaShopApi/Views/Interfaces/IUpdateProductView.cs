namespace AlzaShopApi.Views.Interfaces;

/// <summary>
/// Represents an interface that combines functionalities for identifying a product by its unique identifier
/// and managing its stock quantity.
/// </summary>
/// <remarks>
/// The IUpdateProductView interface consolidates the properties provided by IProductViewId and IProductViewStockQuantity,
/// allowing for the updating of product details, specifically its identifier and stock quantity.
/// This interface is intended for scenarios where updating product information is required.
/// </remarks>
public interface IUpdateProductView : IProductViewId, IProductViewStockQuantity;