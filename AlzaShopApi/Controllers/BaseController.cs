using Microsoft.AspNetCore.Mvc;

namespace AlzaShopApi.Controllers;

/// <summary>
/// Base controller class that provides common error handling functionality for derived controllers.
/// </summary>
/// <remarks>
/// This controller implements exception handling patterns to standardize error responses across the API.
/// </remarks>
public class BaseController : ControllerBase
{
    /// <summary>
    /// Executes an asynchronous operation with exception handling and custom error processing.
    /// </summary>
    /// <typeparam name="T">The type of action result to be returned. Must implement <see cref="IActionResult"/>.</typeparam>
    /// <param name="handler">The asynchronous function to execute that returns the action result.</param>
    /// <param name="errorHandler">An optional function to handle exceptions and potentially return a custom action result.</param>
    /// <returns>
    /// The result of the handler if successful, the result of the errorHandler if an exception occurs and the errorHandler returns a non-null result,
    /// or a default Problem Details response if an exception occurs and no custom error result is provided.
    /// </returns>
    /// <remarks>
    /// This method provides a standardized way to handle exceptions in controller actions while allowing for custom error processing logic.
    /// </remarks>
    protected async Task<T> TryExecute<T>(Func<Task<T>> handler, Func<Exception, T?>? errorHandler = null)
        where T : IActionResult
    {
        try
        {
            return await handler();
        }

        catch (Exception error)
        {
            if (errorHandler != null)
            {
                var result = errorHandler(error);

                if (result != null)
                {
                    return result;
                }
            }
        }

        return (T)(IActionResult)Problem(
            Constants.Messages.Errors.Server.InternalServerErrorDescription,
            title: Constants.Messages.Errors.Server.InternalServerErrorTitle);
    }

    /// <summary>
    /// Executes an asynchronous operation with exception handling and optional error notification.
    /// </summary>
    /// <typeparam name="T">The type of action result to be returned. Must implement <see cref="IActionResult"/>.</typeparam>
    /// <param name="handler">The asynchronous function to execute that returns the action result.</param>
    /// <param name="errorHandler">An optional action to execute when an exception occurs, e.g., for logging purposes.</param>
    /// <returns>
    /// The result of the handler if successful, or a default Problem Details response if an exception occurs.
    /// </returns>
    /// <remarks>
    /// This overload allows for notification of exceptions (like logging) without customizing the error response.
    /// </remarks>
    protected async Task<T> TryExecute<T>(Func<Task<T>> handler, Action<Exception>? errorHandler = null)
        where T : IActionResult
    {
        try
        {
            return await handler();
        }

        catch (Exception error)
        {
            errorHandler?.Invoke(error);
        }

        return (T)(IActionResult)Problem(
            Constants.Messages.Errors.Server.InternalServerErrorDescription,
            title: Constants.Messages.Errors.Server.InternalServerErrorTitle);
    }
}