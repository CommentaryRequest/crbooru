using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;

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

    public async Task AddAsync(PoolVersion version)
    {
        var pool = version.Pool;
        if (version.NewName == null && !version.DescriptionChanged && version.NewState == pool.PostIds) {
            return;
        }

        _context.PoolVersions.Add(version);
        await _context.SaveChangesAsync();
    }
}
