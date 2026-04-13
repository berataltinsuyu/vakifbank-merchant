using VbMerchant.Models;

namespace VbMerchant.Repositories;

public interface IBasvuruRepository
{
  Task<List<Basvurular>> GetAllAsync();
  Task<Basvurular> GetByIdAsync(int id);
  Task<Basvurular> CreateAsync(Basvurular basvuru);
  Task UpdateAsync(Basvurular basvuru);
  Task<bool> ExistsAsync(int id);
}

