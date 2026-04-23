using MediatR;
using EcommerceDemoV1.Domain.Enums;
using EcommerceDemoV1.Application.Common;

namespace EcommerceDemoV1.Application.Features.PromotionRules.Commands.CreatePromotionRule;

public record CreatePromotionRuleCommand(
    string Name,
    string Description,
    PromotionType Type,
    int? ApplyToCategoryId,
    int? ApplyToProductVariantId,
    int? GiftProductVariantId,
    int MinQuantity,
    int FreeQuantity,
    decimal DiscountPercentage,
    int Priority,
    DateTime StartDate,
    DateTime EndDate
) : IRequest<Result<int>>;