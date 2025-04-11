using AlzaShopApi.Models;

namespace AlzaShopApi.Toolkit.Brokers.Interfaces;

public interface IMessageBroker : IDisposable
{
    void Send(ICommand command);
}