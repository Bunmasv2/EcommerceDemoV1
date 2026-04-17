public class CheckoutResultDto
{
    public int OrderId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? PaymentUrl { get; set; }
}