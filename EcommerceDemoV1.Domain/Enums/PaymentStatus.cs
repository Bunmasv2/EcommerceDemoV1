namespace EcommerceDemoV1.Domain.Enums;

public enum PaymentStatus
{
    Pending = 1,      // Chờ thanh toán
    Paid = 2,        // Đã thanh toán
    Failed = 3,      // Thanh toán thất bại
    Refunded = 4     // Đã hoàn tiền
}