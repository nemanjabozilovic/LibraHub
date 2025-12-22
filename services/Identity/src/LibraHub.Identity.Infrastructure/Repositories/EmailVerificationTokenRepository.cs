using LibraHub.Identity.Application.Abstractions;
using LibraHub.Identity.Domain.Tokens;
using LibraHub.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraHub.Identity.Infrastructure.Repositories;

public class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly IdentityDbContext _context;

    public EmailVerificationTokenRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.EmailVerificationTokens
            .FirstOrDefaultAsync(evt => evt.Token == token, cancellationToken);
    }

    public async Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken = default)
    {
        await _context.EmailVerificationTokens.AddAsync(token, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EmailVerificationToken token, CancellationToken cancellationToken = default)
    {
        _context.EmailVerificationTokens.Update(token);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
