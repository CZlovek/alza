using System.ComponentModel.DataAnnotations;
using AlzaShopApi.Services.Interfaces;
using AlzaShopApi.Toolkit.Brokers.Interfaces;
using AlzaShopApi.Views;
using Asp.Versioning;
using Czlovek.RabbitMq.Base.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AlzaShopApi.Controllers.v1;

/// <summary>
/// Provides API endpoints for managing products within the application.
/// </summary>
/// <remarks>
/// Inherits from <see cref="ProductBaseController"/> to leverage common product-related functionality.
/// </remarks>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class ProductController(ILogger<ProductController> logger, IProductService productService, IMessageBroker messageBroker, IRabbitMqDefaultClient rabbitMqDefaultClient)
    : ProductBaseController(logger, productService, messageBroker, rabbitMqDefaultClient)
{
    /// <summary>
    /// Retrieves a product by its identifier.
    /// </summary>
    /// <param name="id">
    /// The identifier of the product to retrieve. Must be a positive integer greater than 0.
    /// </param>
    /// <param name="availableOnly">
    /// Optional filter to include only available products. Defaults to true if not supplied.
    /// </param>
    /// <returns>
    /// An asynchronous operation returning an <see cref="IActionResult"/>.
    /// On success, returns a status code of 200 OK along with the product data as a <see cref="ProductView"/>.
    /// If the product is not found, returns a status code of 204 No Content.
    /// </returns>
    [HttpGet("{id:int}")]
    [Produces("application/json")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ProductView))]
    [SwaggerResponse(StatusCodes.Status204NoContent)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> Get(
        [FromRoute][Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")] int id,
        [FromQuery] bool? availableOnly = true)
    {
        return GetProduct(id, availableOnly);
    }

    /// <summary>
    /// Retrieves a list of products with an optional availability filter.
    /// </summary>
    /// <param name="availableOnly">
    /// Specifies whether to include only available products. If true, only available products are returned.
    /// If false or null, all products are returned regardless of their availability.
    /// </param>
    /// <returns>
    /// An asynchronous operation, which upon completion returns an <see cref="IActionResult"/> that can be:
    /// - A 200 OK response with a list of <see cref="ProductView"/> objects if products are found.
    /// - A 204 No Content response if no products are available.
    /// </returns>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ProductView[]))]
    [SwaggerResponse(StatusCodes.Status204NoContent)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public Task<IActionResult> Get([FromQuery] bool? availableOnly = true)
    {
        return GetProductList(availableOnly: availableOnly);
    }

    /// <summary>
    /// Handles the creation of a new product using the provided product details.
    /// </summary>
    /// <param name="product">
    /// An instance of <see cref="CreateProductView"/> containing the details of the product to be created,
    /// such as name, URL, description, price, and stock quantity.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation, which upon completion returns an
    /// <see cref="IActionResult"/> indicating the result of the operation.
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
    /// Updates the product information in the system.
    /// </summary>
    /// <param name="product">The updated product details.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the update operation.
    /// </returns>
    [HttpPut]
    [Produces("application/json")]
    [Consumes("application/json")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> Put([FromBody] UpdateProductView product)
    {
        return UpdateProduct(product);
    }
}