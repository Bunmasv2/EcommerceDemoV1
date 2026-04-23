using System.Text.Json;
using EcommerceDemoV1.Domain.Entities;

public interface IPayOsService
{
    Task<string> CreatePaymentAsync(Order order, Payment payment);
    Task<PaymentWebhookResult> VerifyPaymentWebhookData(JsonElement webhookBody);
}