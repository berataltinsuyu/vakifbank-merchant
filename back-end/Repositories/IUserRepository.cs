using VbMerchant.Data.Entities;

namespace VbMerchant.Repositories;

public interface IUserRepository
{
    Task<Kullanicilar?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Kullanicilar?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(Kullanicilar kullanici, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
