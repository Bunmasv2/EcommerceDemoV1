public class PaymentWebhookResult
{
    public bool IsSuccess { get; }
    public bool IsCancelled { get; }
    public bool IsExpired { get; }
    public int OrderId { get; }
    public string TransactionId { get; }

    public PaymentWebhookResult(bool isSuccess, bool isCancelled, bool isExpired, int orderId, string transactionId)
    {
        IsSuccess = isSuccess;
        IsCancelled = isCancelled;
        IsExpired = isExpired;
        OrderId = orderId;
        TransactionId = transactionId;
    }
}