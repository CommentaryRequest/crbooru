using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;

namespace CRbooru.Services;

public class TagImplicationException : Exception
{
    public TagImplicationException() : base()
    {
    }

    public TagImplicationException(string message) : base(message)
    {
    }

    public TagImplicationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class TagImplicationService
{
    private readonly CRbooruContext _context;

    public TagImplicationService(CRbooruContext context)
    {
        _context = context;
    }

    public IQueryable<TagImplication> Query()
    {
        return _context.TagImplications
            .OrderByDescending(t => t.Id)
            .Include(t => t.Antecedent)
            .Include(t => t.Consequent);
    }

    public async Task<List<TagImplication>> ListAsync()
    {
        return await Query().ToListAsync();
    }

    public async Task<TagImplication?> GetAsync(int id)
    {
        return await Query().FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<TagImplication>> ByAntecedent(int id)
    {
        return await Query().Where(t => t.AntecedentId == id).ToListAsync();
    }

    public async Task<List<TagImplication>> ByConsequent(int id)
    {
        return await Query().Where(t => t.ConsequentId == id).ToListAsync();
    }

    public async Task<bool> HasLoopedImplications(Tag antecedent, Tag consequent)
    {
        var visited = new HashSet<int>();
        var pending = new Stack<int>();
        pending.Push(consequent.Id);

        while (pending.TryPop(out var current)) {
            if (current == antecedent.Id) {
                return true;
            }

            if (!visited.Add(current)) {
                continue;
            }

            var implications = await Query()
                .Where(t => t.AntecedentId == current)
                .Select(t => t.ConsequentId)
                .ToListAsync();

            foreach (var c in implications) {
                pending.Push(c);
            }
        }

        return false;
    }

    public async Task<TagImplication> AddAsync(Tag antecedent, Tag consequent)
    {
        if (antecedent.Id == consequent.Id) {
            throw new TagImplicationException("Cannot imply a tag to itself");
        }

        if (antecedent.Category != consequent.Category) {
            throw new TagImplicationException("Cannot imply tags with different categories");
        }

        if (Query().Any(t => t.AntecedentId == antecedent.Id && t.ConsequentId == consequent.Id)) {
            throw new TagImplicationException("Implication already exists");
        }

        if (await HasLoopedImplications(antecedent, consequent)) {
            throw new TagImplicationException("Implication would create a loop");
        }

        var implication = new TagImplication {
            AntecedentId = antecedent.Id,
            ConsequentId = consequent.Id
        };
        _context.TagImplications.Add(implication);
        await _context.SaveChangesAsync();

        return implication;
    }
}
