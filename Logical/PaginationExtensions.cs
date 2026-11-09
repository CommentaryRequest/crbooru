using CRbooru.Models;
using Microsoft.EntityFrameworkCore;

namespace CRbooru.Logical;

public static class PaginationExtensions
{
    public static async Task<List<T>> PaginateAsync<T>(this IQueryable<T> query, PaginationInfo pagination)
    {
        return await query
            .Skip(pagination.SkipAmount())
            .Take(pagination.ClampedLimit())
            .ToListAsync();
    }
}