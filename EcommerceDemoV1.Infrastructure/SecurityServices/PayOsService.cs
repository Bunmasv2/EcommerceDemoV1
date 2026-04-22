using System.Text.Json;
using EcommerceDemoV1.Domain.Entities;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using EcommerceDemoV1.Application.DTOs;

public class PayOsService : IPayOsService
{
    private readonly IConfiguration _configuration;
    private readonly PayOSClient _payOsClient;

    public PayOsService(IConfiguration configuration)
    {
        _configuration = configuration;

        var clientId = _configuration["PayOS:ClientId"];
        Console.WriteLine($"[DEBUG PAYOS] ClientId đang dùng là: {clientId}");

        _payOsClient = new PayOSClient(
            _configuration["PayOS:ClientId"] ?? throw new ArgumentNullException("Thiếu PayOS ClientId"),
            _configuration["PayOS:ApiKey"] ?? throw new ArgumentNullException("Thiếu PayOS ApiKey"),
            _configuration["PayOS:ChecksumKey"] ?? throw new ArgumentNullException("Thiếu PayOS ChecksumKey")
        );
    }

    public async Task<string> CreatePaymentAsync(Order order, Payment payment)
    {
        var frontendUrl = "https://localhost:3000";

        var paymentRequest = new CreatePaymentLinkRequest
        {
            OrderCode = order.Id,
            Amount = (int)order.FinalTotal,
            Description = $"Thanh toan don {order.Id}", // Tối đa 25 ký tự, không dấu
            CancelUrl = $"{frontendUrl}/payment/cancel?orderId={order.Id}",
            ReturnUrl = $"{frontendUrl}/payment/success?orderId={order.Id}",
            ExpiredAt = (int)DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds()
        };

        try
        {
            var paymentLink = await _payOsClient.PaymentRequests.CreateAsync(paymentRequest);
            return paymentLink.CheckoutUrl;
        }
        catch (Exception ex)
        {
            throw new Exception($"Không thể tạo link thanh toán PayOS: {ex.Message}");
        }
    }

    public async Task<PaymentWebhookResult> VerifyPaymentWebhookData(JsonElement webhookBody)
    {
        try
        {
            var jsonString = webhookBody.GetRawText();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var payosWebhook = JsonSerializer.Deserialize<Webhook>(jsonString, options);

            if (payosWebhook == null || payosWebhook.Data == null)
                return new PaymentWebhookResult(false, false, false, 0, "");

            var verifiedData = await _payOsClient.Webhooks.VerifyAsync(payosWebhook);

            // 3. Nếu không văng lỗi -> Chữ ký hợp lệ 100%, không bị Hacker giả mạo!
            bool isSuccess = payosWebhook.Code == "00";
            bool isExpired = payosWebhook.Code == "01";
            bool isCancelled = payosWebhook.Code == "02";

            int orderId = (int)verifiedData.OrderCode;
            string transactionId = verifiedData.Reference ?? "UNKNOWN";

            return new PaymentWebhookResult(isSuccess, isCancelled, isExpired, orderId, transactionId);
        }
        catch (Exception ex)
        {
            // Nếu Hacker gửi data giả mạo hoặc sai ChecksumKey, nó sẽ bay vào đây
            Console.WriteLine($"[SECURITY ALERT] Xác thực Webhook thất bại: {ex.Message}");
            return new PaymentWebhookResult(false, false, false, 0, "");
        }
    }
}