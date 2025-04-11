using System.ComponentModel.DataAnnotations;
using AlzaShopApi.Models;
using AlzaShopApi.Models.Database;
using AlzaShopApi.Services.Interfaces;
using AlzaShopApi.Toolkit;
using AlzaShopApi.Toolkit.Brokers.Interfaces;
using AlzaShopApi.Views;
using Czlovek.Json;
using Czlovek.RabbitMq.Base.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace AlzaShopApi.Controllers;

/// <summary>
/// Serves as the base controller for product-related operations and provides shared functionality for derived controllers.
/// </summary>
/// <remarks>
/// This controller includes common methods for handling product data such as retrieval, creation, updating, and queuing product updates.
/// It leverages logging, a product service for business logic, and a message broker for message handling.
/// </remarks>
public class ProductBaseController(
    ILogger<ProductBaseController> logger,
    IProductService productService,
    IMessageBroker messageBroker,
    IRabbitMqDefaultClient rabbitMqDefaultClient)
    : BaseController
{
    /// <summary>
    /// Retrieves a product by its identifier with an option to filter by availability.
    /// </summary>
    /// <param name="id">The unique identifier of the product. Must be a positive integer greater than 0.</param>
    /// <param name="availableOnly">
    /// Specifies whether the product should only be retrieved if available:
    /// - Pass true to return only if the product is in stock.
    /// - Pass false or null to retrieve the product regardless of its stock status.
    /// </param>
    /// <returns>
    /// An IActionResult containing one of the following:
    /// - 200 OK with the product data if found.
    /// - 204 No Content if no product matching the criteria is found.
    /// - 400 Bad Request if the product ID is invalid.
    /// - 500 Internal Server Error if an exception occurs during execution.
    /// </returns>
    protected Task<IActionResult> GetProduct(int id, bool? availableOnly)
    {
        return TryExecute<IActionResult>(async () =>
        {
            if (id < 1)
            {
                return BadRequest("Id must be greater than 0.");
            }

            var product = await productService.GetProduct(id, availableOnly);

            if (product != null)
            {
                return Ok(product.Adapt<ProductView>());
            }

            return NoContent();
        }, error =>
        {
            logger.LogError(error, "Error while getting product with id {ProductId}", id);

            return error switch
            {
                NotFoundException notFoundException => BadRequest(notFoundException.Message),
                ValidationException validationException => BadRequest(validationException.ValidationResult.ToString()),
                _ => null
            };
        });
    }

    /// <summary>
    /// Retrieves a paginated list of products with optional filtering by availability.
    /// </summary>
    /// <param name="pageIndex">
    /// Optional zero-based page index for pagination.
    /// When null, pagination is not applied.
    /// </param>
    /// <param name="pageLimit">
    /// Optional maximum number of items to return per page.
    /// When null, pagination is not applied.
    /// </param>
    /// <param name="availableOnly">
    /// Optional filter to include only available products.
    /// When true, only returns products with positive stock quantity.
    /// When false or null, returns all products regardless of availability.
    /// </param>
    /// <returns>
    /// An action result that contains:
    /// - 200 OK with a list of products if at least one product is found
    /// - 204 No Content if no products match the criteria
    /// - 500 Internal Server Error if an exception occurs during processing
    /// </returns>
    protected Task<IActionResult> GetProductList(int? pageIndex = null, int? pageLimit = null,
        bool? availableOnly = null)
    {
        return TryExecute<IActionResult>(async () =>
        {
            var enumerator = productService.GetProductList(pageIndex, pageLimit, availableOnly).GetAsyncEnumerator();

            var result = new List<ProductView>();

            while (await enumerator.MoveNextAsync())
            {
                result.Add(enumerator.Current.Adapt<ProductView>());
            }

            if (result.Any())
            {
                return Ok(result);
            }

            return NoContent();
        }, error => logger.LogError(error, "Error while getting product list"));
    }

    /// <summary>
    /// Creates a new product based on the provided details.
    /// </summary>
    /// <param name="product">
    /// The data for the product to be created, including required details such as name and URL,
    /// as well as optional fields like price, description, and stock quantity.
    /// </param>
    /// <returns>
    /// An action result that contains:
    /// - 200 OK if the product was successfully created
    /// - 500 Internal Server Error if an error occurs during the creation process
    /// </returns>
    protected Task<IActionResult> CreateProduct(CreateProductView product)
    {
        return TryExecute<IActionResult>(async () =>
        {
            if (string.IsNullOrEmpty(product.Name))
            {
                return BadRequest("Name is required.");
            }

            if (string.IsNullOrEmpty(product.Url) || !Uri.TryCreate(product.Url, UriKind.Absolute, out _))
            {
                return BadRequest("Url is required and must be a valid URL.");
            }

            await productService.CreateProduct(product.Adapt<Product>());

            return Ok();
        }, error =>
        {
            logger.LogError(error, "Error while creating product {ProductName}", product.Name);

            return error is ValidationException validationException
                ? BadRequest(validationException.ValidationResult.ToString())
                : null;
        });
    }

    /// <summary>
    /// Updates an existing product's stock quantity and associated information.
    /// </summary>
    /// <param name="product">
    /// The details of the product to update, including the product ID and the updated stock quantity.
    /// </param>
    /// <returns>
    /// An action result that contains:
    /// - 200 OK if the product was successfully updated
    /// - 400 Bad Request if the specified product could not be found
    /// - 500 Internal Server Error for any other processing errors
    /// </returns>
    protected Task<IActionResult> UpdateProduct(UpdateProductView product)
    {
        return TryExecute<IActionResult>(async () =>
        {
            if (product.Id < 1)
            {
                return BadRequest("Id must be greater than 0");
            }

            await productService.UpdateProductStockQuantity(product.Adapt<Product>());

            return Ok();
        }, error =>
        {
            logger.LogError(error, "Error while updating product {ProductId}", product.Id);

            return error switch
            {
                NotFoundException notFoundException => BadRequest(notFoundException.Message),
                ValidationException validationException => BadRequest(validationException.ValidationResult.ToString()),
                _ => null
            };
        });
    }

    /// <summary>
    /// Enqueues an update to a product's stock quantity by sending a message to the message broker.
    /// </summary>
    /// <param name="product">
    /// The product update data encapsulated in an <see cref="UpdateProductView"/>.
    /// Must include a valid product ID and updated fields.
    /// </param>
    /// <returns>
    /// An action result that contains:
    /// - 200 OK when the update message is enqueued successfully
    /// - 400 Bad Request if the product ID is invalid or the product does not exist
    /// - 500 Internal Server Error if an exception occurs during the operation
    /// </returns>
    protected Task<IActionResult> EnqueueUpdateProduct(UpdateProductView product)
    {
        return TryExecute<IActionResult>(async () =>
        {
            if (product.Id < 1)
            {
                return BadRequest("Id must be greater than 0.");
            }

            await productService.EnsureProductExists(product.Id);

            messageBroker.Send(product.Adapt<UpdateProductStockQuantityCommand>());

            return Ok();
        }, error =>
        {
            logger.LogError(error, "Error while updating product {ProductId}", product.Id);

            return error switch
            {
                NotFoundException notFoundException => BadRequest(notFoundException.Message),
                ValidationException validationException => BadRequest(validationException.ValidationResult.ToString()),
                _ => null
            };
        });
    }

    protected Task<IActionResult> RabbitMqUpdateProduct(UpdateProductView product)
    {
        return TryExecute<IActionResult>(async () =>
        {
            if (product.Id < 1)
            {
                return BadRequest("Id must be greater than 0.");
            }

            await productService.EnsureProductExists(product.Id);

            var json = product.Adapt<UpdateProductStockQuantityCommand>().Serialize();

            await rabbitMqDefaultClient.Send(Constants.RabbitMq.Exchange, Constants.RabbitMq.RoutingKey, json);

            return Ok();
        }, error =>
        {
            logger.LogError(error, "Error while updating product {ProductId}", product.Id);

            return error switch
            {
                NotFoundException notFoundException => BadRequest(notFoundException.Message),
                ValidationException validationException => BadRequest(validationException.ValidationResult.ToString()),
                _ => null
            };
        });
    }
}
