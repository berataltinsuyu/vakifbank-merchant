using Microsoft.EntityFrameworkCore;
using VbMerchant.Data;
using VbMerchant.Models;
using VbMerchant.Repositories;

namespace VbMerchant.Repositories;

public class BasvuruRepository : IBasvuruRepository
{
private readonly AppDbContext _context;

public BasvuruRepository(AppDbContext context)
{
_context = context;
}
public async Task<List<Basvurular>> GetAllAsync()
{
return await _context.Basvurulars
.Include(b => b.SirketTipi) 
.Include(b => b.Il) 
.Include(b => b.Ilce) 
.Include(b => b.BasvuruDokumanlaris) 
.OrderByDescending(b => b.OlusturmaTarihi)
.ToListAsync();
}
public async Task<Basvurular?> GetByIdAsync(int id)
{
return await _context.Basvurulars
.Include(b => b.SirketTipi)
.Include(b => b.Il)
.Include(b => b.Ilce)
.Include(b => b.BasvuruDokumanlaris)
.Include(b => b.BasvuruTarihces) 
.FirstOrDefaultAsync(b => b.Id == id);

}
public async Task<Basvurular> CreateAsync(Basvurular basvuru)
{
_context.Basvurulars.Add(basvuru); 
await _context.SaveChangesAsync(); 
return basvuru; 
}
public async Task UpdateAsync(Basvurular basvuru)
{
_context.Basvurulars.Update(basvuru);
await _context.SaveChangesAsync();
}
public async Task<bool> ExistsAsync(int id)
{

return await _context.Basvurulars.AnyAsync(b => b.Id == id);
}
} 