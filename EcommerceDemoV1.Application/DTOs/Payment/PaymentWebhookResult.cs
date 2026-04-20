public class PaymentWebhookResult
{
    public bool IsSuccess { get; }
    public int OrderId { get; }
    public string TransactionId { get; }

    public PaymentWebhookResult(bool isSuccess, int orderId, string transactionId)
    {
        IsSuccess = isSuccess;
        OrderId = orderId;
        TransactionId = transactionId;
    }
}