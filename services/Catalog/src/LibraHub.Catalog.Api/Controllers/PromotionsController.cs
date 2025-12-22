using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.BuildingBlocks.Results;
using LibraHub.Catalog.Api.Dtos.Promotions;
using LibraHub.Catalog.Application.Promotions.Commands.ActivateCampaign;
using LibraHub.Catalog.Application.Promotions.Commands.AddPromotionRule;
using LibraHub.Catalog.Application.Promotions.Commands.CancelCampaign;
using LibraHub.Catalog.Application.Promotions.Commands.CreatePromotionCampaign;
using LibraHub.Catalog.Application.Promotions.Commands.EndCampaign;
using LibraHub.Catalog.Application.Promotions.Commands.PauseCampaign;
using LibraHub.Catalog.Application.Promotions.Commands.RemovePromotionRule;
using LibraHub.Catalog.Application.Promotions.Queries.GetCampaign;
using LibraHub.Catalog.Application.Promotions.Queries.GetCampaigns;
using LibraHub.Catalog.Domain.Promotions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraHub.Catalog.Api.Controllers;

[ApiController]
[Route("promotions")]
[Authorize(Roles = "Librarian,Admin")]
public class PromotionsController(
    IMediator mediator,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("campaigns")]
    [ProducesResponseType(typeof(GetCampaignsResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampaigns(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCampaignsQuery(page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("campaigns")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateCampaign(
        [FromBody] CreatePromotionCampaignRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.UserId.HasValue)
        {
            return Unauthorized();
        }

        if (!Enum.TryParse<StackingPolicy>(request.StackingPolicy, ignoreCase: true, out var stackingPolicy))
        {
            return BadRequest(new { code = "INVALID_STACKING_POLICY", message = "Invalid stacking policy" });
        }

        var command = new CreatePromotionCampaignCommand(
            request.Name,
            request.Description,
            request.StartsAtUtc,
            request.EndsAtUtc,
            stackingPolicy,
            request.Priority,
            currentUser.UserId.Value);

        var result = await mediator.Send(command, cancellationToken);
        return result.ToCreatedActionResult(this, nameof(GetCampaign), new { id = result.Value });
    }

    [HttpGet("campaigns/{id}")]
    [ProducesResponseType(typeof(GetCampaignResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampaign(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCampaignQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("campaigns/{id}/rules")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddRule(
        Guid id,
        [FromBody] AddPromotionRuleRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<DiscountType>(request.DiscountType, ignoreCase: true, out var discountType))
        {
            return BadRequest(new { code = "INVALID_DISCOUNT_TYPE", message = "Invalid discount type" });
        }

        if (!Enum.TryParse<PromotionScope>(request.AppliesToScope, ignoreCase: true, out var scope))
        {
            return BadRequest(new { code = "INVALID_SCOPE", message = "Invalid promotion scope" });
        }

        var command = new AddPromotionRuleCommand(
            id,
            discountType,
            request.DiscountValue,
            request.Currency,
            request.MinPriceAfterDiscount,
            request.MaxDiscountAmount,
            scope,
            request.ScopeValues,
            request.Exclusions);

        var result = await mediator.Send(command, cancellationToken);
        return result.ToCreatedActionResult(this, nameof(GetCampaign), new { id });
    }

    [HttpDelete("rules/{ruleId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveRule(
        [FromQuery] Guid campaignId,
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        var command = new RemovePromotionRuleCommand(campaignId, ruleId);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToNoContentActionResult(this);
    }

    [HttpPost("campaigns/{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ActivateCampaign(Guid id, CancellationToken cancellationToken)
    {
        if (!currentUser.UserId.HasValue)
        {
            return Unauthorized();
        }

        var command = new ActivateCampaignCommand(id, currentUser.UserId.Value);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToNoContentActionResult(this);
    }

    [HttpPost("campaigns/{id}/pause")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PauseCampaign(Guid id, CancellationToken cancellationToken)
    {
        if (!currentUser.UserId.HasValue)
        {
            return Unauthorized();
        }

        var command = new PauseCampaignCommand(id, currentUser.UserId.Value);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToNoContentActionResult(this);
    }

    [HttpPost("campaigns/{id}/end")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> EndCampaign(Guid id, CancellationToken cancellationToken)
    {
        if (!currentUser.UserId.HasValue)
        {
            return Unauthorized();
        }

        var command = new EndCampaignCommand(id, currentUser.UserId.Value);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToNoContentActionResult(this);
    }

    [HttpPost("campaigns/{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CancelCampaign(Guid id, CancellationToken cancellationToken)
    {
        if (!currentUser.UserId.HasValue)
        {
            return Unauthorized();
        }

        var command = new CancelCampaignCommand(id, currentUser.UserId.Value);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToNoContentActionResult(this);
    }
}
