public record CouponCalculationDto(
    string CouponCode,
    decimal DiscountAmount,
    decimal FinalTotal
);