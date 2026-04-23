using MediatR;
using EcommerceDemoV1.Application.Common;

namespace EcommerceDemoV1.Application.Features.Shipment.Queries.GetShippingEstimate;

public record GetShippingEstimateQuery(
    string DeliveryAddress,
    string ReceiverPhone,
    double? WeightKg = 1,
    string? ServiceId = null
) : IRequest<Result<GetShippingEstimateResponse>>;

public record GetShippingEstimateResponse(
    string ServiceId,
    decimal ShippingFee,
    double Distance,
    int EstimatedDuration
);
