using EcommerceDemoV1.Domain.Entities;
using EcommerceDemoV1.Domain.Enums;
using Microsoft.EntityFrameworkCore;

public class ShipmentRepository : IShipmentRepository
{
    private readonly AppDbContext _context;

    public ShipmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Shipment?> GetShipmentByAhamoveOrderIdAsync(string ahamoveOrderId)
    {
        return await _context.Shipments
            .FirstOrDefaultAsync(s => s.AhamoveOrderId == ahamoveOrderId);
    }
}