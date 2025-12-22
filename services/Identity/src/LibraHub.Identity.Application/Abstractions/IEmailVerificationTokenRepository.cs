using LibraHub.Identity.Domain.Tokens;

namespace LibraHub.Identity.Application.Abstractions;

public interface IEmailVerificationTokenRepository
{
    Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken = default);

    Task UpdateAsync(EmailVerificationToken token, CancellationToken cancellationToken = default);
}
