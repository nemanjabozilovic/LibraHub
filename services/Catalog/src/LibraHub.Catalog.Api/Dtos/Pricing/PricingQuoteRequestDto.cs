namespace LibraHub.Catalog.Api.Dtos.Pricing;

public record PricingQuoteRequestDto
{
    public string Currency { get; init; } = string.Empty;
    public List<PricingQuoteItemRequestDto> Items { get; init; } = new();
    public DateTime? AtUtc { get; init; }
}
