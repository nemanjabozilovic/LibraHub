using FluentValidation;

namespace LibraHub.Identity.Application.Auth.Commands.VerifyEmail;

public class VerifyEmailValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required");
    }
}
