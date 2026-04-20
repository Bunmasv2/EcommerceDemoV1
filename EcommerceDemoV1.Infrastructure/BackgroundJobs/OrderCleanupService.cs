using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using EcommerceDemoV1.Domain.Enums;
using Microsoft.Extensions.Logging;
using EcommerceDemoV1.Domain.Entities;

namespace EcommerceDemoV1.Infrastructure.BackgroundJobs;

public class OrderCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OrderCleanupService> _logger;
    public OrderCleanupService(IServiceScopeFactory serviceScopeFactory, ILogger<OrderCleanupService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order Cleanup Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                    var variantRepository = scope.ServiceProvider.GetRequiredService<IProductVariantRepository>();

                    var expiredTime = DateTime.UtcNow.AddMinutes(-15);
                    var expiredOrders = await orderRepository.GetExpiredPendingOrdersAsync(expiredTime);

                    if (expiredOrders.Any())
                    {
                        _logger.LogInformation($"Found {expiredOrders.Count} expired pending orders. Cleaning up...");

                        await unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, stoppingToken);

                        foreach (var order in expiredOrders)
                        {
                            order.Status = OrderStatus.Cancelled;
                            order.PaymentStatus = PaymentStatus.Failed;

                            var variantIds = order.Items.Select(i => i.ProductVariantId).ToList();
                            var variantsFromDb = await variantRepository.GetListByIdsAsync(variantIds);
                            var variantDict = variantsFromDb.ToDictionary(v => v.Id);
                            Console.WriteLine($"Test transaction rollback AAAAAA: {order.Items}");

                            foreach (var item in order.Items)
                            {
                                if (variantDict.TryGetValue(item.ProductVariantId, out var variant))
                                {
                                    variant.StockQuantity += item.Quantity;
                                    await variantRepository.UpdateAsync(variant);
                                }
                            }
                            _logger.LogInformation($"Cancelled order {order.Id} and restocked items.");
                        }
                        await unitOfWork.SaveChangesAsync(stoppingToken);
                        await unitOfWork.CommitTransactionAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while cleaning up expired orders: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}