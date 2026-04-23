using EcommerceDemoV1.Domain.Entities;
using EcommerceDemoV1.Domain.Enums;


public interface IShipmentRepository
{
    Task<Shipment?> GetShipmentByAhamoveOrderIdAsync(string ahamoveOrderId);
}