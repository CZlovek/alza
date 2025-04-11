using AlzaShopApi.Services.Interfaces;
using AlzaShopApi.Toolkit.Brokers.Interfaces;
using AlzaShopApi.Views;
using Asp.Versioning;
using Czlovek.RabbitMq.Base.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AlzaShopApi.Controllers.v2;

/// <summary>
/// Provides API endpoints for managing products with pagination support (API v2).
/// </summary>
/// <remarks>
/// This controller extends <see cref="ProductBaseController"/> to implement the version 2 of the Products API,
/// which adds pagination capabilities to the product listing functionality.
/// </remarks>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
public class ProductController(ILogger<ProductController> logger, IProductService productService, IMessageBroker messageBroker, IRabbitMqDefaultClient rabbitMqDefaultClient)
    : ProductBaseController(logger, productService, messageBroker, rabbitMqDefaultClient)
{
    /// <summary>
    /// Retrieves a specific product by its unique identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the product to retrieve. Must be a positive integer greater than 0.
    /// </param>
    /// <param name="availableOnly">
    /// Optional flag to filter for only available products. If true (default), only returns products that are in stock.
    /// If false, returns the product regardless of its availability status.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing an <see cref="IActionResult"/> that:
    /// - Returns 200 OK with product data if found
    /// - Returns 204 No Content if the product is not found
    /// </returns>
    [HttpGet("{id:int}")]
    [Produces("application/json")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ProductView))]
    [SwaggerResponse(StatusCodes.Status204NoContent)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> Get(
        [FromRoute] int id,
        [FromQuery] bool? availableOnly = true)
    {
        return GetProduct(id, availableOnly);
    }

    /// <summary>
    /// Retrieves a paginated list of products with optional filtering by availability.
    /// </summary>
    /// <param name="pageIndex">
    /// The zero-based page index to retrieve. Must be a non-negative integer.
    /// </param>
    /// <param name="pageLimit">
    /// The maximum number of items to include per page. Must be a positive integer greater than 0.
    /// </param>
    /// <param name="availableOnly">
    /// Optional flag to filter for only available products. If true (default), only returns products that are in stock.
    /// If false or null, returns all products regardless of their availability status.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing an <see cref="IActionResult"/> that:
    /// - Returns 200 OK with a list of product data if products are found
    /// - Returns 204 No Content if no products match the criteria
    /// </returns>
    [HttpGet]
    [Produces("application/json")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ProductView[]))]
    [SwaggerResponse(StatusCodes.Status204NoContent)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> Get(
        [FromQuery] int? pageIndex,
        [FromQuery] int? pageLimit,
        [FromQuery] bool? availableOnly = true)
    {
        return GetProductList(pageIndex, pageLimit ?? Constants.Paging.PageSize, availableOnly);
    }

    /// <summary>
    /// Creates a new product in the system.
    /// </summary>
    /// <param name="product">
    /// The product details required to create a new product entry.
    /// Must include required fields like Name and URL, while other fields like Price,
    /// StockQuantity, and Description are optional.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing an <see cref="IActionResult"/>
    /// indicating the success of the operation.
    /// </returns>
    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> Post([FromBody] CreateProductView product)
    {
        return CreateProduct(product);
    }

    /// <summary>
    /// Updates an existing product in the system.
    /// </summary>
    /// <param name="product">
    /// The product update details. Must include the product ID and at least one field to update.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing an <see cref="IActionResult"/>
    /// indicating the success of the operation.
    /// </returns>
    [HttpPut]
    [Produces("application/json")]
    [Consumes("application/json")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> Put([FromBody] UpdateProductView product)
    {
        return EnqueueUpdateProduct(product);
    }
}
