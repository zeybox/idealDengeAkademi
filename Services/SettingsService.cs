using HizliOgren.Data;
using Microsoft.EntityFrameworkCore;

namespace HizliOgren.Services;

public class SettingsService
{
    private readonly ApplicationDbContext _db;

    public SettingsService(ApplicationDbContext db) => _db = db;

    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        var row = await _db.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key, ct);
        return row?.Value;
    }

    public async Task<IReadOnlyDictionary<string, string?>> GetManyAsync(IEnumerable<string> keys, CancellationToken ct = default)
    {
        var list = keys.Distinct().ToList();
        if (list.Count == 0) return new Dictionary<string, string?>();
        var rows = await _db.Settings.AsNoTracking().Where(s => list.Contains(s.Key)).ToListAsync(ct);
        return rows.ToDictionary(r => r.Key, r => r.Value);
    }

    public async Task SetAsync(string key, string? value, CancellationToken ct = default)
    {
        var row = await _db.Settings.FirstOrDefaultAsync(s => s.Key == key, ct);
        if (row != null)
        {
            row.Value = value;
            await _db.SaveChangesAsync(ct);
        }
        else
        {
            _db.Settings.Add(new Models.Setting { Key = key, Value = value });
            await _db.SaveChangesAsync(ct);
        }
    }
}
