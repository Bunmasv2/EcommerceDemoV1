using MediatR;
using FluentValidation;
using EcommerceDemoV1.Application.Features.Shipment.Queries.GetShippingEstimate;
using EcommerceDemoV1.Application.Features.Shipment.Commands.ProcessAhaMoveWebhook;

namespace EcommerceDemoV1.Api.Endpoints;

public static class ShipmentEndpoints
{
    public static void MapShipmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/shipments")
            .WithTags("Shipment - Vận chuyển");

        group.MapPost("/estimate", async (
            GetShippingEstimateQuery query,
            IMediator mediator,
            IValidator<GetShippingEstimateQuery> validator) =>
        {
            var validationResult = await validator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.ToDictionary());
            }

            var result = await mediator.Send(query);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { success = false, message = result.ErrorMessage });
            }
            return Results.Ok(new { success = true, data = result.Data, message = "Lấy phí vận chuyển thành công" });
        })
        .RequireAuthorization()
        .WithName("GetShippingEstimate")
        .WithDescription("Lấy phí vận chuyển ước tính từ Ahamove dựa trên địa chỉ giao hàng và cân nặng hàng");

        group.MapPost("/webhook", async (
            ProcessAhaMoveWebhookCommand command,
            IMediator mediator
            ) =>
        {
            // var validationResult = await validator.ValidateAsync(command);
            // if (!validationResult.IsValid)
            // {
            //     return Results.BadRequest(validationResult.ToDictionary());
            // }

            var result = await mediator.Send(command);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { success = false, message = result.ErrorMessage });
            }
            return Results.Ok(new { success = true, data = result.Data, message = result.Data?.Message });
        })
        .WithName("ProcessAhaMoveWebhook")
        .WithDescription(@"
            Webhook endpoint để Ahamove gửi cập nhật trạng thái vận đơn.
            
            Trạng thái có thể là:
            - IDLE: Vận đơn vừa tạo
            - ACCEPTED: Tài xế đã nhận
            - IN_PROCESS: Đang vận chuyển
            - COMPLETED: Giao hàng thành công
            - CANCELLED: Hủy vận đơn
            
            Khi ACCEPTED: Sẽ cập nhật tên/SĐT tài xế vào Shipment
            Khi COMPLETED: Sẽ cập nhật Order.Status thành Delivered
        ");
    }
}
