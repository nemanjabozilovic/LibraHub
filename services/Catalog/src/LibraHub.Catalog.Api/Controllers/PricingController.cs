using LibraHub.BuildingBlocks.Results;
using LibraHub.Catalog.Api.Dtos.Pricing;
using LibraHub.Catalog.Application.Promotions.Queries.GetPricingQuote;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraHub.Catalog.Api.Controllers;

[ApiController]
[Route("pricing")]
public class PricingController(IMediator mediator) : ControllerBase
{
    [HttpPost("quote")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PricingQuoteResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuote(
        [FromBody] PricingQuoteRequestDto request,
        CancellationToken cancellationToken)
    {
        var items = request.Items.Select(i => new PricingQuoteItemRequest { BookId = i.BookId }).ToList();
        var query = new GetPricingQuoteQuery(request.Currency, items, request.AtUtc);
        var result = await mediator.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }
}
