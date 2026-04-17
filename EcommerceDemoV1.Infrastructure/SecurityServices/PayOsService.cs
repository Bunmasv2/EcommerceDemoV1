using EcommerceDemoV1.Domain.Entities;

public class PayOsService : IPayOsService
{
    public async Task<string> CreatePaymentAsync(Order order, Payment payment)
    {
        // TẠM THỜI GÁN MOCK DATA (Dữ liệu giả)

        await Task.Delay(500);

        // Trả về một link thanh toán giả để test luồng Checkout
        return $"https://pay.payos.vn/mock-link-test-for-order-{order.Id}";
    }
}