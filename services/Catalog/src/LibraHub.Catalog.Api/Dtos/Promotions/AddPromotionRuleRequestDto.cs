namespace LibraHub.Catalog.Api.Dtos.Promotions;

public record AddPromotionRuleRequestDto
{
    public string DiscountType { get; init; } = string.Empty;
    public decimal DiscountValue { get; init; }
    public string? Currency { get; init; }
    public decimal? MinPriceAfterDiscount { get; init; }
    public decimal? MaxDiscountAmount { get; init; }
    public string AppliesToScope { get; init; } = string.Empty;
    public List<string>? ScopeValues { get; init; }
    public List<Guid>? Exclusions { get; init; }
}
