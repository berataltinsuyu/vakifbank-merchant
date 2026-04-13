using VbMerchant.DTOs;

namespace VbMerchant.Services;
public interface IBasvuruService
{
Task<List<BasvuruResponse>> GetAllAsync();
Task<BasvuruResponse?> GetByIdAsync(int id);
Task<BasvuruResponse> CreateAsync(BasvuruCreateRequest request);

Task UpdateDurumAsync(int id, string yeniDurum, string kullanici);
}

