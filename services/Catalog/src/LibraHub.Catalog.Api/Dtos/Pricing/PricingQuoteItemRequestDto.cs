namespace LibraHub.Catalog.Api.Dtos.Pricing;

public record PricingQuoteItemRequestDto
{
    public Guid BookId { get; init; }
}
