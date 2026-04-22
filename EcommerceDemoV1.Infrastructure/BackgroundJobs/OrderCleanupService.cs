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

    public OrderCleanupService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OrderCleanupService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order Cleanup Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredOrdersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up expired orders.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcessExpiredOrdersAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var variantRepository = scope.ServiceProvider.GetRequiredService<IProductVariantRepository>();

        var expiredTime = DateTime.UtcNow.AddMinutes(-15);
        var expiredOrders = await orderRepository.GetExpiredPendingOrdersAsync(expiredTime);

        if (!expiredOrders.Any())
            return;

        _logger.LogInformation("Found {Count} expired orders. Processing...", expiredOrders.Count);

        var allVariantIds = expiredOrders
            .SelectMany(o => o.Items)
            .Select(i => i.ProductVariantId)
            .Distinct()
            .ToList();

        var allVariants = await variantRepository.GetListByIdsAsync(allVariantIds);
        var variantDict = allVariants.ToDictionary(v => v.Id);

        await unitOfWork.BeginTransactionAsync(
            System.Data.IsolationLevel.RepeatableRead, stoppingToken);
        try
        {
            foreach (var order in expiredOrders)
            {
                order.Status = OrderStatus.Cancelled;
                order.PaymentStatus = PaymentStatus.Failed;

                foreach (var item in order.Items)
                {
                    if (variantDict.TryGetValue(item.ProductVariantId, out var variant))
                        variant.StockQuantity += item.Quantity; // EF tự track
                }

                _logger.LogInformation(
                    "Cancelled order {OrderId}, restocked {Count} item types.",
                    order.Id, order.Items.Count);
            }

            await unitOfWork.SaveChangesAsync(stoppingToken);
            await unitOfWork.CommitTransactionAsync(stoppingToken);

            _logger.LogInformation(
                "Successfully processed {Count} expired orders.", expiredOrders.Count);
        }
        catch (Exception)
        {
            await unitOfWork.RollbackTransactionAsync(stoppingToken);
            throw;
        }
    }
}