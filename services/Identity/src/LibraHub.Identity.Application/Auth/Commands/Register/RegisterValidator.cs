using FluentValidation;
using LibraHub.Identity.Domain.Users;

namespace LibraHub.Identity.Application.Auth.Commands.Register;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .Must(PasswordPolicy.IsValid)
            .WithMessage(PasswordPolicy.GetPolicyDescription());
    }
}
