using Hangfire;
using Microsoft.EntityFrameworkCore;
using VbMerchant.Data;

namespace VbMerchant.Jobs;

public class VbMerchantJobService
{
    private readonly AppDbContext _db;
    private readonly Hangfire.IRecurringJobManager _recurringJobManager;

    public VbMerchantJobService(AppDbContext db, Hangfire.IRecurringJobManager recurringJobManager)
    {
        _db = db;
        _recurringJobManager = recurringJobManager;
    }

    public async Task UpdateBasvuruDurumAsync()
    {
        var basvurular = await _db.Basvurulars
            .Where(b => b.Durum == "Beklemede" && b.OlusturmaTarihi <= DateTime.Now.AddDays(-2))
            .ToListAsync();

        foreach (var basvuru in basvurular)
        {
            basvuru.Durum = "Iptal Edildi";
        }

        await _db.SaveChangesAsync();
    }

    public void ScheduleJobs()
    {
        _recurringJobManager.AddOrUpdate(
            "basvuru-durum-guncelleme",
            Hangfire.Common.Job.FromExpression<VbMerchantJobService>(
                x => x.UpdateBasvuruDurumAsync()),
            "*/30 * * * *");
    }
}
