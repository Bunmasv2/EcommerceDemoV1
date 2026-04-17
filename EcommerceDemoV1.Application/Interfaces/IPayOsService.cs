using EcommerceDemoV1.Domain.Entities;

public interface IPayOsService
{
    Task<string> CreatePaymentAsync(Order order, Payment payment);
}