using Microsoft.EntityFrameworkCore;
using VbMerchant.Data;
using VbMerchant.Data.Entities;

namespace VbMerchant.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Kullanicilar?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (normalizedEmail is null)
        {
            return null;
        }

        return await _context.Kullanicilars
            .FirstOrDefaultAsync(k => k.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public async Task<Kullanicilar?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (normalizedEmail is null)
        {
            return null;
        }

        return await _context.Kullanicilars
            .FirstOrDefaultAsync(
                k => k.IsActive && k.Email.ToLower() == normalizedEmail,
                cancellationToken);
    }

    public async Task AddAsync(Kullanicilar kullanici, CancellationToken cancellationToken = default)
    {
        await _context.Kullanicilars.AddAsync(kullanici, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    private static string? NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim().ToLowerInvariant();
    }
}
