using AlzaShopApi.Models;
using AlzaShopApi.Models.Database;
using AlzaShopApi.Services.Interfaces;
using Czlovek.Json;
using Czlovek.RabbitMq.Base.Interfaces;
using Mapster;
using System.Text;

namespace AlzaShopApi.Services
{
    public class RabbitMqUpdateService : BackgroundService
    {
        private readonly IRabbitMqDefaultClient _rabbitMqDefaultClient;
        private readonly ILogger<RabbitMqUpdateService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public RabbitMqUpdateService(IRabbitMqDefaultClient rabbitMqDefaultClient, ILogger<RabbitMqUpdateService> logger, IServiceProvider serviceProvider)
        {
            _rabbitMqDefaultClient = rabbitMqDefaultClient;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        private void RegisterRabbitMqUpdateHandler()
        {
            try
            {
                _rabbitMqDefaultClient.Register("Alza.Shop.Commands.Update", async (content, _) =>
                {
                    try
                    {
                        await using var scope = _serviceProvider.CreateAsyncScope();
                        using var productService = scope.ServiceProvider.GetService<IProductService>();

                        if (productService != null)
                        {
                            await productService.UpdateProductStockQuantity(Encoding.UTF8.GetString(content.Span)
                                .Deserialize<UpdateProductStockQuantityCommand>().Adapt<Product>());
                        }

                        return true;
                    }

                    catch (Exception error)
                    {
                        _logger.LogError(error, "Error while processing Update command");

                        return false;
                    }
                });
            }

            catch (Exception error)
            {
                _logger.LogError(error, "Error while registering listener");

            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Yield();

            RegisterRabbitMqUpdateHandler();

            return Task.CompletedTask;
        }
    }
}
