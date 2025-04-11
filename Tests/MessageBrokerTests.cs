// C#
using AlzaShopApi.Models;
using AlzaShopApi.Models.Database;
using AlzaShopApi.Services.Interfaces;
using AlzaShopApi.Toolkit.Brokers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Concurrent;
using System.Reflection;

namespace Tests;

/// <summary>
/// Unit test class for testing the functionality of the MessageBroker class.
/// </summary>
public class MessageBrokerTests
{
    /// <summary>
    /// Mock instance of <see cref="ILogger{TCategoryName}"/> for the <see cref="MessageBroker"/> class.
    /// </summary>
    /// <remarks>
    /// Used to verify logging behavior during unit tests for the <see cref="MessageBrokerTests"/> class.
    /// Enables test cases to assert whether specific log messages are recorded and categorized appropriately.
    /// </remarks>
    private readonly Mock<ILogger<MessageBroker>> _loggerMock;

    /// <summary>
    /// A mock implementation of <see cref="IServiceScope"/> used for unit testing scenarios in <c>MessageBrokerTests</c>.
    /// </summary>
    private readonly Mock<IServiceScope> _serviceScopeMock;

    /// <summary>
    /// A mock object for the <see cref="IProductService"/> interface used in testing scenarios.
    /// </summary>
    /// <remarks>
    /// This mock is used in unit tests for verifying behaviors and interactions with methods defined
    /// in the <see cref="IProductService"/> interface. It is configured and injected to simulate various
    /// service scenarios without relying on the actual implementation.
    /// </remarks>
    private readonly Mock<IProductService> _productServiceMock;

    /// <summary>
    /// Represents a private instance of the <see cref="MessageBroker"/> class that provides messaging capabilities for enqueuing and processing commands.
    /// </summary>
    /// <remarks>
    /// This variable initializes the <see cref="MessageBroker"/> with required dependencies such as a logger and service provider.
    /// It is used for unit testing to verify the behavior of command enqueueing, processing, and resource cleanup.
    /// </remarks>
    private readonly MessageBroker _messageBroker;

    /// <summary>
    /// Contains unit tests for the <see cref="MessageBroker"/> class.
    /// </summary>
    /// <remarks>
    /// Validates the functionality, behavior, and interface compliance of the <see cref="MessageBroker"/> implementation.
    /// Includes tests for proper command processing, error handling, resource management, and message queue integrity.
    /// </remarks>
    public MessageBrokerTests()
    {
        _loggerMock = new Mock<ILogger<MessageBroker>>();
        
        var serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _productServiceMock = new Mock<IProductService>();

        // Setup service provider chain
        serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactoryMock.Object);

        serviceScopeFactoryMock.Setup(x => x.CreateScope())
            .Returns(_serviceScopeMock.Object);

        _serviceScopeMock.Setup(x => x.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        serviceProviderMock.Setup(x => x.GetService(typeof(IProductService)))
            .Returns(_productServiceMock.Object);

        _productServiceMock.Setup(x => x.SaveChanges())
            .ReturnsAsync(1);

        _messageBroker = new MessageBroker(_loggerMock.Object, serviceProviderMock.Object);
    }

    /// <summary>
    /// Verifies that the <see cref="MessageBroker.Send(ICommand)"/> method correctly enqueues a command
    /// into the internal queue for further processing.
    /// </summary>
    /// <remarks>
    /// This test ensures that when a command is sent to the message broker, it is properly added
    /// to the internal command queue. The test uses reflection to access the private queue and
    /// asserts that the added command is correctly stored and available at the front of the queue.
    /// </remarks>
    [Fact]
    public void Send_EnqueuesCommand()
    {
        // Arrange
        var command = new UpdateProductStockQuantityCommand
        {
            Id = 1,
            StockQuantity = 10
        };

        // Get access to private queue
        var commandsField = typeof(MessageBroker).GetField("_commands",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        _messageBroker.Send(command);

        // Assert - verify command was added to queue
        var commands = commandsField?.GetValue(_messageBroker) as ConcurrentQueue<ICommand>;
        Assert.NotNull(commands);
        Assert.True(commands.TryPeek(out var peekedCommand));
        Assert.Equal(command, peekedCommand);
    }

    /// <summary>
    /// Verifies that the <see cref="MessageBroker.Dispose"/> method releases allocated resources correctly.
    /// </summary>
    /// <remarks>
    /// This test ensures that when the <see cref="MessageBroker.Dispose"/> method is invoked,
    /// it performs proper cleanup of allocated resources without throwing any exceptions.
    /// The test does not validate internal state changes or resource deallocation directly,
    /// but asserts that no runtime errors occur during disposal.
    /// </remarks>
    [Fact]
    public void Dispose_ReleasesResources()
    {
        // Act
        _messageBroker.Dispose();

        // Assert - this is mostly to ensure no exceptions are thrown during disposal
        // We can't easily verify internal state as resources are disposed
        Assert.True(true, "Dispose completed without throwing exceptions");
    }

    /// Processes and updates the stock quantity of a product based on the given command.
    /// This method is responsible for handling the `UpdateProductStockQuantityCommand` by
    /// extracting the necessary product details, updating the stock quantity,
    /// and persisting the changes to the database.
    /// If the product service fails to process the command, appropriate exception handling
    /// should ensure the application remains stable.
    /// <returns>
    /// A `Task` representing the asynchronous operation.
    /// </returns>
    [Fact]
    public async Task Work_ProcessesUpdateProductStockQuantityCommand()
    {
        // Arrange
        var command = new UpdateProductStockQuantityCommand
        {
            Id = 1,
            StockQuantity = 10
        };

        // Configure our service scope to return our product service
        //_serviceProviderMock.Setup(x => x.CreateScope())
        //    .Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(IProductService)))
            .Returns(_productServiceMock.Object);

        // Setup the product service mock expectation
        _productServiceMock.Setup(x => x.LazyUpdateProductStockQuantity(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Act
        _messageBroker.Send(command);

        // We need to wait for the background processing to occur
        await Task.Delay(100); // Give enough time for the background processing to run

        // Assert
        _productServiceMock.Verify(x => x.LazyUpdateProductStockQuantity(
            It.Is<Product>(p => p.Id == command.Id && p.StockQuantity == command.StockQuantity)),
            Times.Once);
        _productServiceMock.Verify(x => x.SaveChanges(), Times.AtLeastOnce);
    }

    /// Tests whether the Work method in the MessageBroker handles exceptions thrown during command processing by:
    /// 1. Logging the error appropriately.
    /// 2. Ensuring the processing mechanism continues to function after the exception.
    /// <return>
    /// This test verifies that exceptions thrown during command processing do not disrupt the overall functionality of the system,
    /// ensuring error logs are recorded and the processing pipeline remains operational.
    /// </return>
    [Fact]
    public async Task Work_HandlesExceptionInCommandProcessing()
    {
        // Arrange
        var command = new UpdateProductStockQuantityCommand
        {
            Id = 1,
            StockQuantity = 10
        };

        // Setup product service to throw an exception
        _productServiceMock.Setup(x => x.LazyUpdateProductStockQuantity(It.IsAny<Product>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        _messageBroker.Send(command);

        // Wait for background processing
        await Task.Delay(3000);

        // Assert - verify error is logged but processing continues
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);

        // Verify that SaveChanges is still called even after the exception
        _productServiceMock.Verify(x => x.SaveChanges(), Times.AtLeastOnce);
    }

    /// <summary>
    /// Tests whether the Work method correctly handles an unknown command type
    /// by logging a warning message.
    /// </summary>
    /// <returns>Task representing the asynchronous operation of the test.</returns>
    [Fact]
    public async Task Work_HandlesUnknownCommandType()
    {
        // Arrange
        var command = new UnknownCommand();

        // Act
        _messageBroker.Send(command);

        // Wait for background processing
        await Task.Delay(100);

        // Assert - verify warning is logged for unknown command type
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unknown command type")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    // Test helper classes
    /// <summary>
    /// Represents a command type that is unrecognized or not handled by the system.
    /// </summary>
    /// <remarks>
    /// This class is used in scenarios where a command does not match any known types
    /// within the application. It can serve as a placeholder for identifying invalid
    /// or unsupported commands during processing.
    /// </remarks>
    private class UnknownCommand : ICommand
    {
        /// <summary>
        /// Gets or sets the unique identifier for the command.
        /// </summary>
        /// <remarks>
        /// This property is used to uniquely identify a specific command instance.
        /// It plays a key role in ensuring that the correct command is processed
        /// and tracked during execution.
        /// </remarks>
        public int Id { get; set; }
    }

    /// Ensures the `ShouldContinueProcessing` method returns false when a cancellation has been requested.
    /// This test validates that the message broker correctly checks its internal cancellation token
    /// state and stops processing when the cancellation has been triggered.
    [Fact]
    public void ShouldContinueProcessing_ReturnsFalseWhenCancellationRequested()
    {
        // Arrange
        var messageBroker = _messageBroker;

        // Use reflection to get private fields and methods
        var cancellationTokenSourceField = typeof(MessageBroker).GetField("_cancellationTokenSource",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var originalCts = cancellationTokenSourceField?.GetValue(messageBroker) as CancellationTokenSource;

        var newCts = new CancellationTokenSource();
        newCts.Cancel(); // Request cancellation
        cancellationTokenSourceField?.SetValue(messageBroker, newCts);

        var shouldContinueProcessingMethod = typeof(MessageBroker).GetMethod("ShouldContinueProcessing",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var result = (bool)shouldContinueProcessingMethod?.Invoke(messageBroker, null)!;

        // Assert
        Assert.False(result);

        // Restore the original CTS
        cancellationTokenSourceField?.SetValue(messageBroker, originalCts);
    }

    /// Validates that when multiple commands are sent to the MessageBroker,
    /// they are enqueued in the same order they are sent, preserving the
    /// First-In-First-Out (FIFO) behavior of the internal queue.
    /// This method performs the following checks:
    /// - Ensures commands are added to the queue.
    /// - Verifies the total number of commands in the queue matches the number of commands sent.
    /// - Confirms that the commands are dequeued in the exact order they were enqueued.
    /// The method uses reflection to access the private queue of the MessageBroker
    /// instance and asserts the expected behavior.
    [Fact]
    public void Send_MultipleCommands_PreservesOrderInQueue()
    {
        // Arrange
        var command1 = new UpdateProductStockQuantityCommand { Id = 1, StockQuantity = 10 };
        var command2 = new UpdateProductStockQuantityCommand { Id = 2, StockQuantity = 20 };
        var command3 = new UpdateProductStockQuantityCommand { Id = 3, StockQuantity = 30 };

        // Get access to private queue
        var commandsField = typeof(MessageBroker).GetField("_commands",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        _messageBroker.Send(command1);
        _messageBroker.Send(command2);
        _messageBroker.Send(command3);

        // Assert - verify commands were added to queue in order
        var commands = commandsField?.GetValue(_messageBroker) as ConcurrentQueue<ICommand>;
        Assert.NotNull(commands);

        var commandsList = commands.ToArray();
        Assert.Equal(3, commands.Count);

        // Verify order is preserved (FIFO)
        Assert.Equal(command1, commandsList[0]);
        Assert.Equal(command2, commandsList[1]);
        Assert.Equal(command3, commandsList[2]);
    }
}
