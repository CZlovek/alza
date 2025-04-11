using System.Collections.Concurrent;
using System.ComponentModel;
using AlzaShopApi.Models;
using AlzaShopApi.Models.Database;
using AlzaShopApi.Services.Interfaces;
using AlzaShopApi.Toolkit.Brokers.Interfaces;
using Mapster;

namespace AlzaShopApi.Toolkit.Brokers;

/// <summary>
/// Represents a broker that facilitates the sending and processing of commands within the application.
/// </summary>
public class MessageBroker : IMessageBroker
{
    /// <summary>
    /// Represents a synchronization primitive used to block threads until a signal is received, enabling thread-safe
    /// coordination within the <see cref="MessageBroker"/> for processing commands in a concurrent environment.
    /// </summary>
    /// <remarks>
    /// This <see cref="AutoResetEvent"/> instance ensures that the message processing logic in <see cref="MessageBroker"/>
    /// can effectively wait for and respond to new commands being enqueued, resuming execution only after a signal is set.
    /// </remarks>
    private readonly AutoResetEvent _autoResetEvent = new(false);

    /// <summary>
    /// Represents a background worker used for executing commands in the message broker asynchronously.
    /// This worker utilizes the <see cref="System.ComponentModel.BackgroundWorker"/> class to handle execution
    /// of long-running or continuous tasks in a separate thread, ensuring that the main thread remains responsive.
    /// </summary>
    /// <remarks>
    /// - The <c>_worker</c> is a private member of the <c>MessageBroker</c> class and is initialized upon object creation.
    /// - It is responsible for orchestrating task execution in a loop until cancellation is requested.
    /// - Cancellation and state monitoring are handled effectively through the combination of background worker capabilities and other synchronization primitives (e.g., <see cref="System.Threading.AutoResetEvent"/>).
    /// - The worker's cancellation can be triggered via the <c>Dispose</c> method.
    /// </remarks>
    private readonly BackgroundWorker _worker = new();

    /// <summary>
    /// Represents a token source used to manage and signal cancellation for asynchronous operations within the <see cref="MessageBroker"/> class.
    /// </summary>
    /// <remarks>
    /// This <see cref="CancellationTokenSource"/> is primarily utilized to signal termination of background worker threads
    /// and ensures proper cleanup of resources when the <see cref="MessageBroker"/> is disposed.
    /// </remarks>
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    /// <summary>
    /// Represents a thread-safe queue designed to manage and store commands to be processed asynchronously within the application.
    /// </summary>
    /// <remarks>
    /// This queue is used internally by the <see cref="MessageBroker"/> to manage instances
    /// of <see cref="ICommand"/>. Commands stored in this queue are processed in the order
    /// they are added, ensuring sequential execution of operations.
    /// </remarks>
    private readonly ConcurrentQueue<ICommand> _commands = new();

    /// <summary>
    /// Logger instance for the <see cref="MessageBroker"/> class.
    /// Used to log informational messages, warnings, and errors during execution
    /// of command processing and other operations within the <see cref="MessageBroker"/>.
    /// </summary>
    private readonly ILogger<MessageBroker> _logger;

    /// <summary>
    /// A semaphore used to limit the number of threads that can access a
    /// critical section of code simultaneously within the <see cref="MessageBroker"/> class.
    /// </summary>
    /// <remarks>
    /// This instance of <see cref="SemaphoreSlim"/> is initialized with a
    /// concurrency level of 1, ensuring that only one thread at a time
    /// can perform operations requiring synchronized access.
    /// </remarks>
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Provides an instance of <see cref="IServiceProvider"/> to manage service lifetimes
    /// and dependencies within the <see cref="MessageBroker"/> class.
    /// Used to create service scopes and resolve services such as <see cref="IProductService"/>
    /// dynamically during the execution of commands.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;


    /// <summary>
    /// Provides a mechanism for managing and processing commands asynchronously.
    /// </summary>
    /// <remarks>
    /// The <see cref="MessageBroker"/> class is responsible for enqueuing, executing, and managing
    /// lifecycle activities for commands that implement the <see cref="ICommand"/> interface.
    /// Commands are processed in the background using a dedicated worker thread.
    /// </remarks>
    public MessageBroker(ILogger<MessageBroker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        _worker.WorkerSupportsCancellation = true;

        _ = EnsureExecution();
    }

    /// <summary>
    /// Sends a command to be processed by the message broker.
    /// </summary>
    /// <param name="command">The command to enqueue for processing.</param>
    public void Send(ICommand command)
    {
        _commands.Enqueue(command);

        _autoResetEvent.Set();
    }

    /// <summary>
    /// Ensures the execution of the background worker responsible for processing commands.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation to ensure the background worker is executing.
    /// </returns>
    private async Task EnsureExecution()
    {
        await _semaphore.WaitAsync();

        if (!_worker.IsBusy)
        {
            Execute();

            _semaphore.Release();
        }
    }


    /// <summary>
    /// Starts the background process for executing queued commands.
    /// </summary>
    /// <remarks>
    /// This method initializes the background worker responsible for processing
    /// commands stored in the concurrent queue. It continuously processes commands until
    /// cancellation is requested or a related error occurs. Errors during execution are logged
    /// for diagnostics without stopping the process.
    /// </remarks>
    private void Execute()
    {
        _worker.DoWork += async (_, _) =>
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    await Work();
                }

                catch (Exception error)
                {
                    _logger.LogError(error, "Failed to process events.");
                }
            }
        };

        _worker.RunWorkerAsync();
    }


    /// <summary>
    /// Processes commands from a queue and executes appropriate actions
    /// based on the command type using services resolved via the provided service scope.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task Work()
    {
        while (_autoResetEvent.WaitOne() && ShouldContinueProcessing())
        {
            using var scope = _serviceProvider.CreateScope();
            var productService = scope.ServiceProvider.GetService<IProductService>();

            while (productService != null && ShouldContinueProcessing() && _commands.TryDequeue(out var command))
            {
                try
                {
                    switch (command)
                    {
                        case UpdateProductStockQuantityCommand updateStockCommand:
                            _logger.LogInformation(
                                $"Updating stock for product {updateStockCommand.Id} to {updateStockCommand.StockQuantity}");

                            await productService.LazyUpdateProductStockQuantity(updateStockCommand.Adapt<Product>());

                            break;

                        default:
                            _logger.LogWarning($"Unknown command type: {command.GetType()}");
                            break;
                    }
                }
                catch (Exception error)
                {
                    _logger.LogError(error, "Failed to process command.");
                }
            }

            await productService!.SaveChanges();
        }
    }

    private bool ShouldContinueProcessing()
    {
        return !_cancellationTokenSource.IsCancellationRequested && !_worker.CancellationPending;
    }

    /// <summary>
    /// Releases all resources used by the <see cref="MessageBroker"/>.
    /// </summary>
    /// <remarks>
    /// This method ensures that resources allocated by the <see cref="MessageBroker"/>
    /// such as cancellation tokens, reset events, and semaphores are disposed of properly.
    /// It is recommended to call this method when the <see cref="MessageBroker"/>
    /// is no longer needed to free up resources promptly.
    /// </remarks>
    public void Dispose()
    {
        _autoResetEvent.Set();

        _cancellationTokenSource.Cancel();
        _worker.CancelAsync();
        _autoResetEvent.Dispose();
        _semaphore.Dispose();
        _cancellationTokenSource.Dispose();
    }
}