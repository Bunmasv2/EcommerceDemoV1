using MediatR;
using EcommerceDemoV1.Application.Common;
using Microsoft.Extensions.Options;
using EcommerceDemoV1.Domain.Common;

namespace EcommerceDemoV1.Application.Features.Shipment.Queries.GetShippingEstimate;

public class GetShippingEstimateQueryHandler : IRequestHandler<GetShippingEstimateQuery, Result<GetShippingEstimateResponse>>
{
    private readonly IAhamoveService _ahamoveService;
    private readonly AhamoveSettings _ahamoveSettings;

    public GetShippingEstimateQueryHandler(IAhamoveService ahamoveService, IOptions<AhamoveSettings> ahamoveSettings)
    {
        _ahamoveService = ahamoveService;
        _ahamoveSettings = ahamoveSettings.Value;
    }

    public async Task<Result<GetShippingEstimateResponse>> Handle(
        GetShippingEstimateQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var pickupLocation = new Location(
                Latitude: _ahamoveSettings.Latitude,
                Longitude: _ahamoveSettings.Longitude,
                Address: _ahamoveSettings.Address
            );

            var deliveryLocation = await _ahamoveService.GeocodeAddressAsync(request.DeliveryAddress);

            var estimateResponse = await _ahamoveService.EstimateOrderFeeAsync(
                pickupLocation: pickupLocation,
                deliveryLocation: deliveryLocation,
                weightKg: request.WeightKg ?? 1,
                serviceId: request.ServiceId
            );

            var response = new GetShippingEstimateResponse(
                ServiceId: estimateResponse.ServiceId,
                ShippingFee: estimateResponse.TotalPay,
                Distance: estimateResponse.Distance,
                EstimatedDuration: estimateResponse.EstimatedDuration
            );

            return Result<GetShippingEstimateResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<GetShippingEstimateResponse>.Failure($"Lỗi khi lấy phí vận chuyển: {ex.Message}");
        }
    }
}
