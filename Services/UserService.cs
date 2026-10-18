using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;

namespace CRbooru.Services;

public class UserService
{
    private readonly CRbooruContext _context;

    public UserService(CRbooruContext context)
    {
        _context = context;
    }

    public ValueTask<User?> Get(int id)
    {
        return _context.Users.FindAsync(id);
    }

    public async Task<User?> ByName(string name)
    {
        return await _context.Users.FirstOrDefaultAsync(u => name == u.Name);
    }

    public async Task<User?> GetCurrentUser(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true) {
            return null;
        }

        string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) {
            return null;
        }
        return await Get(int.Parse(userId));
    }

    public bool Any()
    {
        return _context.Users.Any();
    }

    public async Task Add(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<List<User>> ListAsync()
    {
        return await _context.Users.ToListAsync();
    }
}
