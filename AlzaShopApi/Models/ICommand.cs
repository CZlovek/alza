namespace AlzaShopApi.Models;

/// <summary>
/// Represents a command that can be executed within the application.
/// </summary>
/// <remarks>
/// This interface is intended to serve as a marker interface for command objects
/// that encapsulate the data and behavior for executing specific operations.
/// Implementing this interface indicates that a class represents a command
/// to be processed by a handler or message broker.
/// </remarks>
public interface ICommand;
