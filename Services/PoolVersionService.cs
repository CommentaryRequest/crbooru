using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Logical;

namespace CRbooru.Services;

public class PoolVersionService
{
    private CRbooruContext _context;

    public PoolVersionService(CRbooruContext context)
    {
        _context = context;
    }

    private IQueryable<PoolVersion> getPoolVersions()
    {
        return _context.PoolVersions
.Include(v => v.Updater)
            .Include(v => v.Pool);
    }

    public async Task<PoolVersion?> Get(int id)
    {
        return await getPoolVersions().FirstOrDefaultAsync(v => v.Id == id);
    }

    public IQueryable<PoolVersion> Query(int? poolId, int? updaterId)
    {
        IQueryable<PoolVersion> query = getPoolVersions();

        if (poolId != null) {
            query = query.Where(v => v.PoolId == poolId);
        }
        if (updaterId != null) {
            query = query.Where(v => v.UpdaterId == updaterId);
        }

        return query;
    }

    public List<PoolVersion> List(int? poolId, int? updaterId, bool orderAscending = false)
    {
        IQueryable<PoolVersion> query = Query(poolId, updaterId);
        query = orderAscending
            ? query.OrderBy(v => v.Id)
            : query.OrderByDescending(v => v.Id);
        return query.ToList();
    }

    public async Task AddAsync(PoolVersion version, bool bypassValidation = false)
    {
        var pool = version.Pool;
        if (!bypassValidation &&
            version.NewName == null &&
            !version.DescriptionChanged &&
            version.NewState == pool.PostIds) {
            return;
        }

        var versions = List(pool.Id, null, true);
        if (versions.Any()) {
            var lastVersion = versions[^1];
            if (lastVersion.UpdaterId == version.UpdaterId &&
                DateTime.UtcNow - lastVersion.CreatedAt < TimeSpan.FromHours(1)) {
                PoolVersion mergedVersion = PoolVersionMerger.Merge(lastVersion, version, versions);
                lastVersion.NewState = mergedVersion.NewState;
                lastVersion.NewName = mergedVersion.NewName;
                lastVersion.NewDescription = mergedVersion.NewDescription;
                lastVersion.DescriptionChanged = mergedVersion.DescriptionChanged;
                await _context.SaveChangesAsync();
                return;
            }
        }

        _context.PoolVersions.Add(version);
        await _context.SaveChangesAsync();
    }
}
