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

    public async Task<TagImplication> AddAsync(Tag Antecedent, Tag Consequent)
    {
        if (Antecedent.Id == Consequent.Id) {
            throw new TagImplicationException("Cannot imply a tag to itself");
        }

        if (Antecedent.Category != Consequent.Category) {
            throw new TagImplicationException("Cannot imply tags with different categories");
        }

        var implication = new TagImplication {
            AntecedentId = Antecedent.Id,
            ConsequentId = Consequent.Id
        };
        _context.TagImplications.Add(implication);
        await _context.SaveChangesAsync();

        return implication;
    }
}
