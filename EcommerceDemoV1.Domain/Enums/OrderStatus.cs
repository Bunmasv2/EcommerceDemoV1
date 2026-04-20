namespace EcommerceDemoV1.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Paid = 2,
    Confirmed = 3,
    Shipping = 4,
    Completed = 5,
    Cancelled = 6,
    Processing = 7, // Thêm trạng thái mới cho đơn hàng đã thanh toán nhưng chưa giao hàng
}