using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CRbooru.Models;

public enum PostRating : byte
{
    General,
    Sensitive,
    Questionable,
    Explicit
}

public class Post
{
    public int Id { get; private set; }
    public User Uploader { get; private set; }
    public MediaAsset MediaAsset { get; set; }
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public PostRating Rating { get; set; } = PostRating.General;
    public string? Source { get; set; } = "";

    private Post()
    {
    }

    public Post(User uploader, MediaAsset mediaAsset, PostRating rating, string source)
    {
        Uploader = uploader;
        MediaAsset = mediaAsset;
        Tags = new List<Tag>();
        Rating = rating;
        Source = source;
    }
}
