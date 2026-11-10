using CRbooru.Data;
using CRbooru.Models;
using Microsoft.EntityFrameworkCore;

namespace CRbooru.Services;

public class ArtistCommentaryException : Exception
{
    public ArtistCommentaryException(string message) : base(message)
    {
    }
}

public class ArtistCommentaryService
{
    private CRbooruContext _context;
    private PostService _posts;

    public ArtistCommentaryService(CRbooruContext context, PostService posts)
    {
        _context = context;
        _posts = posts;
    }

    public IQueryable<ArtistCommentary> Query()
    {
        return _context.ArtistCommentaries
            .OrderByDescending(c => c.Id);
    }

    public async Task<ArtistCommentary?> ByPost(int postId)
    {
        return await Query().FirstOrDefaultAsync(c => c.PostId == postId);
    }
    
    public async Task CreateOrUpdateAsync(User user, int postId, string? originalTitle, string? originalDescription, string? translatedTitle, string? translatedDescription)
    {
        var commentary = await ByPost(postId);
        if (commentary == null) {
            var post = await _posts.Get(postId);
            if (post == null) {
                throw new ArtistCommentaryException($"Post #{postId} does not exist");
            }

            commentary = new ArtistCommentary {
                PostId = postId,
                Post = post,
                OriginalTitle = originalTitle,
                OriginalDescription = originalDescription,
                TranslatedTitle = translatedTitle,
                TranslatedDescription = translatedDescription
            };
            _context.ArtistCommentaries.Add(commentary);
        } else {
            commentary.OriginalTitle = originalTitle;
            commentary.OriginalDescription = originalDescription;
            commentary.TranslatedTitle = translatedTitle;
            commentary.TranslatedDescription = translatedDescription;
        }

        await _context.SaveChangesAsync();
    }
}