using FluentValidation;

namespace LibraHub.Catalog.Application.Promotions.Commands.CreatePromotionCampaign;

public class CreatePromotionCampaignValidator : AbstractValidator<CreatePromotionCampaignCommand>
{
    public CreatePromotionCampaignValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Campaign name is required")
            .MaximumLength(200).WithMessage("Campaign name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.StartsAtUtc)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndsAtUtc)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartsAtUtc).WithMessage("End date must be after start date");

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0).WithMessage("Priority must be non-negative");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("Created by user ID is required");
    }
}
